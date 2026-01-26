using HarmonyLib;
using RimWorld;
using UnityEngine;
using UranvManosaba.Contents.Comps;
using Verse;
using Verse.AI.Group;
using Debug = System.Diagnostics.Debug;

namespace UranvManosaba.Contents.Utils;

public static class YukiGeneralUtils
{
    /// <summary>
    /// 从请求生成月代雪
    /// </summary>
    public static Pawn GenerateYukiPawn(bool isColonist = false, QualityCategory qualityEq =  QualityCategory.Normal)
    {
        var faction = Faction.OfPlayer;
        if (!isColonist)
        {
            faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Ancients) 
                      ?? Find.FactionManager.RandomNonHostileFaction(false, false, false);
        }

        var pawnKind = isColonist ? ModDefOf.UmPawnKindYukiColonist : ModDefOf.UmPawnKindYukiVisitor;
        
        var request = new PawnGenerationRequest(
            kind: pawnKind,
            faction: faction,
            context: PawnGenerationContext.NonPlayer,
            tile: -1,
            forceGenerateNewPawn: true,
            allowDead: false,
            allowDowned: false,
            canGeneratePawnRelations: false,
            mustBeCapableOfViolence: true,
            colonistRelationChanceFactor: 0f,
            forceAddFreeWarmLayerIfNeeded: false,
            allowGay: true,
            allowPregnant: false,
            allowFood: true,
            allowAddictions: false,
            inhabitant : false,
            worldPawnFactionDoesntMatter: true,
            biocodeApparelChance: 1f,
            fixedBiologicalAge: 15f+0.9f*Rand.Value,
            fixedChronologicalAge: 150f+850f*Rand.Value,
            fixedGender: Gender.Female,
            forceNoBackstory: true,
            forceNoGear: true
        );
        
        var yuki = PawnGenerator.GeneratePawn(request);
        
        ApplyCustomPawnSettings(yuki, isColonist, qualityEq);
        
        return yuki;
    }
    private static void ApplyCustomPawnSettings(Pawn p, bool isColonist = true,  QualityCategory qualityEq = QualityCategory.Normal)
    {
        // 姓名
        CheckYukiName(p,true);
        // 背景
        p.story.Childhood = ModDefOf.UmBackstoryYukiChild;
        if (isColonist) p.story.Adulthood = ModDefOf.UmBackstoryYukiAdult;
        // 文化
        if (ModsConfig.IdeologyActive && p.Ideo == null)
        {
            if (p.Faction?.ideos?.PrimaryIdeo != null)
            {
                p.ideo.SetIdeo(p.Faction.ideos.PrimaryIdeo);
            }
            else if (Faction.OfPlayer.ideos != null && Faction.OfPlayer.ideos.PrimaryIdeo != null)
            {
                p.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
            }
        }
        // 外观
        p.story.hairDef = HairDefOf.Bald;
        p.story.HairColor = Color.white;// Color(233/255f,236/255f,254/255f);
        p.story.bodyType = BodyTypeDefOf.Thin;
        p.story.skinColorOverride = Color.white;//new Color(251/255f,254/255f,255/255f);
        p.story.headType = ModDefOf.UmHeadTypeYuki;
        if (p.style != null)
        {
            p.style.FaceTattoo = TattooDefOf.NoTattoo_Face;
            p.style.BodyTattoo = TattooDefOf.NoTattoo_Body;
        }
        p.Drawer.renderer.SetAllGraphicsDirty();
        // 特性
        p.story.traits.allTraits.Clear();
        p.story.traits.GainTrait(new Trait(TraitDefOf.PerfectMemory));
        p.story.traits.GainTrait(new Trait(TraitDefOf.Gay));
        // 技能
        foreach (var skill in p.skills.skills)
        {
            skill.Level = 18;
            skill.passion = Passion.None;
        }
        // 清空装备
        p.equipment.DestroyAllEquipment();
        p.apparel.DestroyAll();
        p.inventory.DestroyAll();
        // 添加武器
        if (isColonist)
        {
            var weapon = ThingMaker.MakeThing(ModDefOf.UmThingWeaponRitualSword);
            weapon.TryGetComp<CompQuality>()?.SetQuality(qualityEq, ArtGenerationContext.Outsider);
            var compBladelinkWeapon = weapon.TryGetComp<CompBladelinkWeapon>();
            if (compBladelinkWeapon!=null)
            {
                List<WeaponTraitDef> traits = AccessTools.FieldRefAccess<CompBladelinkWeapon, List<WeaponTraitDef>>("traits")(compBladelinkWeapon);
                traits?.Clear();
                Debug.Assert(traits != null, nameof(traits) + " != null");
                traits.Add(DefDatabase<WeaponTraitDef>.GetNamed("OnKill_PsyfocusGain", false));
                traits.Add(DefDatabase<WeaponTraitDef>.GetNamed("NeverBond", false));
            }
            AccessTools.Method(typeof(CompBladelinkWeapon), "CodeFor").Invoke(compBladelinkWeapon, new object[] { p });
            p.equipment.AddEquipment((ThingWithComps)weapon);
        }
        // 添加衣服
        var cloth = ThingMaker.MakeThing(ModDefOf.UmThingApparelYukiDress);
        cloth.TryGetComp<CompQuality>()?.SetQuality(qualityEq, ArtGenerationContext.Outsider);
        var compBiocodable = cloth.TryGetComp<CompBiocodable>();
        AccessTools.Method(typeof(CompBiocodable), "CodeFor").Invoke(compBiocodable, new object[] { p });
        p.apparel.Wear((Apparel)cloth, locked: true);
        // 添加 Hediff
        p.health.AddHediff(ModDefOf.UmHediffYukiDummy);
        //if (!isColonist)
        //{
        //    p.health.AddHediff(ModDefOf.UmHediffYukiVisitor);
        //    p.health.AddHediff(ModDefOf.UmHediffYukiVisitorHidden);
        //}
        //else
        //{
        //    p.health.AddHediff(ModDefOf.UmHediffYukiColonist);
        //    p.health.AddHediff(ModDefOf.UmHediffYukiColonistHidden);
        //}
        // 非殖民者添加对话组件
        if (!isColonist && !p.HasComp<Comp_YukiVisitor>())
        {
            var comps = AccessTools.FieldRefAccess<ThingWithComps, List<ThingComp>>("comps")(p);
            
            var newComp = new Comp_YukiVisitor();
            newComp.parent = p; 
            var props = new CompProperties_YukiVisitor(); 
            newComp.Initialize(props);
            comps.Add(newComp);
            
            if (p.Spawned)
            {
                newComp.PostSpawnSetup(false);
            }
        }
        
    }
    // 检查 Name
    public static void CheckYukiName(Pawn p, bool forced = false)
    {
        if (!forced && p.Name != null) return;
        if (ManosabaMod.Settings.debugMode && p.Name == null) Log.Error($"[Manosaba] {p.def} has null Name (Utils.YukiGeneralUtils.CheckYukiName)");
        var firstName = "YukiGenerator_firstName".CanTranslate() ? "YukiGenerator_firstName".Translate().ToString() : "Yuki";
        var nickName = "YukiGenerator_nickName".CanTranslate() ?  "YukiGenerator_nickName".Translate().ToString() : "Yuki";
        var lastName = "YukiGenerator_lastName".CanTranslate() ? "YukiGenerator_lastName".Translate().ToString() : "Tsukishiro";
        p.Name = new NameTriple(firstName, nickName, lastName);
    }
    // 瞬移离开地图
    public static void LeaveMapInstantly(Pawn p, DestroyMode mode = DestroyMode.Vanish, bool applyMood = false)
    {
        if (p is not { Spawned: true }) return;
        var map = p.Map;
        if (map == null) return;
        var pos = p.Position;
        // vfx
        EffecterDefOf.MeatExplosion.Spawn(pos, map).Cleanup();
        GenClamor.DoClamor(p, pos, 10, ClamorDefOf.Ability);
        // 心情减益
        if (applyMood && !p.IsColonist) AddThoughtToAllColonists(map, ModDefOf.UmThoughtYukiLeaveInstantly);
        // 移除 Pawn 并通知 Group
        p.GetLord()?.Notify_PawnLost(p, PawnLostCondition.LeftVoluntarily, null);
        // 推迟到当前 tick 结束执行，规避一些日志记录 pawn def 问题
        LongEventHandler.ExecuteWhenFinished(() =>
        {
            if (p.Spawned) p.DeSpawn(mode);
        });

        //p.Destroy();
        // 音效
        //SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(pos, map));
        // // 视觉闪光 (FlashEntry)
        // var offset = new Vector3(0,0,0);//(-0.5f, 0f, +0.5f);
        // var flashEntryOverlay = FleckMaker.GetDataAttachedOverlay(p, FleckDefOf.PsycastSkipFlashEntry, offset);
        // flashEntryOverlay.link.detachAfterTicks = 5;
        // map.flecks.CreateFleck(flashEntryOverlay);
        // //FleckMaker.Static(pos, map, FleckDefOf.PsycastSkipFlashEntry);
        // // 空间扭曲 (InnerExit)
        // var innerExitOverlay = FleckMaker.GetDataAttachedOverlay(p, FleckDefOf.PsycastSkipInnerExit, offset);
        // innerExitOverlay.link.detachAfterTicks = 0;
        // map.flecks.CreateFleck(innerExitOverlay);
        // // 外环 (OuterRingExit)
        // var outerRingExitOverlay = FleckMaker.GetDataAttachedOverlay(p, FleckDefOf.PsycastSkipOuterRingExit, offset);
        // outerRingExitOverlay.link.detachAfterTicks = 0;
        // map.flecks.CreateFleck(outerRingExitOverlay);
    }
    private static void AddThoughtToAllColonists(Map map, ThoughtDef thoughtDef)
    {
        if (map == null || thoughtDef == null)
        {
            if (ManosabaMod.Settings.debugMode) Log.Warning("[Manosaba] Map or ThoughtDef is null. (Utils.YukiGeneralUtils.AddThoughtToAllColonists)");
            return;
        }
        var colonists = map.mapPawns.FreeColonists;
        if (colonists is null || colonists.Count <= 0) return;
            
        foreach (var p in colonists)
        {
            if (p.needs is { mood: not null })
            {
                p.needs.mood.thoughts.memories.TryGainMemory(thoughtDef);
            }
        }
    }
}