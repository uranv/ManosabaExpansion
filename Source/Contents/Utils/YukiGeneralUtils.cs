using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using UranvManosaba.Contents.Comps;
using Verse;
using Verse.AI.Group;
using Debug = System.Diagnostics.Debug;

namespace UranvManosaba.Contents.Utils;

public static class YukiGeneralUtils
{
    public static bool IsYuki(Pawn p) => p != null && p.def == ModDefOf.UmThingRaceYuki;

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
        p.story.hairDef = ModDefOf.UmHairYuki;
        p.story.HairColor = Color.white;// Color(233/255f,236/255f,254/255f);
        // p.story.bodyType = BodyTypeDefOf.Thin;
        p.story.bodyType = ManosabaMod.Settings.UseUniqueBodyType? ModDefOf.UmBodyTypeYuki : BodyTypeDefOf.Thin;  // 默认直接使用Thin体型，兼容选项使用独立体型 v1.0.6
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
        // 在添加回调维护状态前，明确指定生成类型。
        var yukiDummy = HediffMaker.MakeHediff(ModDefOf.UmHediffYukiDummy, p);
        yukiDummy.TryGetComp<HediffComp_YukiDummy>().Form = isColonist ? YukiForm.Col : YukiForm.Vis;
        p.health.AddHediff(yukiDummy);

        // 非殖民者添加对话组件
        // if (!isColonist && !p.HasComp<Comp_YukiVisitor>())
        // {
        //     var comps = AccessTools.FieldRefAccess<ThingWithComps, List<ThingComp>>("comps")(p);
        //     
        //     var newComp = new Comp_YukiVisitor();
        //     newComp.parent = p;
        //     var props = new CompProperties_YukiVisitor();
        //     newComp.Initialize(props);
        //     comps.Add(newComp);
        //     
        //     if (p.Spawned)
        //     {
        //         newComp.PostSpawnSetup(false);
        //     }
        // }

    }
    // 检查 Name
    public static void CheckYukiName(Pawn p, bool forced = false)
    {
        if (!forced && p.Name != null) return;
        if (ManosabaMod.Settings.DebugMode && p.Name == null) Log.Error($"[Manosaba] {p.def} has null Name (Utils.YukiGeneralUtils.CheckYukiName)");
        var firstName = "YukiGenerator_firstName".CanTranslate() ? "YukiGenerator_firstName".Translate().ToString() : "Yuki";
        var nickName = "YukiGenerator_nickName".CanTranslate() ?  "YukiGenerator_nickName".Translate().ToString() : "Yuki";
        var lastName = "YukiGenerator_lastName".CanTranslate() ? "YukiGenerator_lastName".Translate().ToString() : "Tsukishiro";
        p.Name = new NameTriple(firstName, nickName, lastName);
    }
    // 是否已登记离场
    public static bool IsLeaving(Pawn p) => Current.Game.GetComponent<ManosabaGameComponent>().IsDepartureQueued(p);

    // 登记瞬移离场，已登记时返回 false；撤离在当前 tick 结束后执行，避开伤害、健康和 Job 回调栈
    public static bool LeaveMapInstantly(Pawn p, bool applyMood = false) =>
        Current.Game.GetComponent<ManosabaGameComponent>().TryQueueDeparture(p, applyMood);

    // 离场发信：有所属地图时关联离场位置；远行队或世界容器内照常发信，不关联地图
    public static void SendLeaveLetter(Pawn p, TaggedString label, TaggedString text, LetterDef letterDef)
    {
        var target = p.MapHeld != null ? new TargetInfo(p.PositionHeld, p.MapHeld) : LookTargets.Invalid;
        Find.LetterStack.ReceiveLetter(label, text, letterDef, target);
    }

    // 执行离场：特效和掉落取 MapHeld 位置，地图内容器按所属容器追溯；Lord、远行队及销毁实例与有无地图无关
    public static void Depart(Pawn p, bool applyMood)
    {
        var map = p.MapHeld;
        var pos = p.PositionHeld;
        var caravan = p.GetCaravan();
        if (map != null) PlayLeaveEffects(p, map, pos, applyMood);
        if (p.IsColonist) HandOverPossessions(p, map, pos, caravan);
        LeaveLord(p);
        if (caravan != null) LeaveCaravan(p, caravan);
        DestroyInstance(p);
    }

    // 地图表现：特效、喧哗及访客离场心情
    private static void PlayLeaveEffects(Pawn p, Map map, IntVec3 pos, bool applyMood)
    {
        EffecterDefOf.MeatExplosion.Spawn(pos, map).Cleanup();
        GenClamor.DoClamor(p, pos, 10, ClamorDefOf.Ability);
        if (applyMood && !p.IsColonist) AddThoughtToAllColonists(map, ModDefOf.UmThoughtYukiLeaveInstantly);
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

    // 物资：殖民者全部装备收入背包后，有地图时在所属位置掉落并禁用，否则转交远行队或所在世界容器
    private static void HandOverPossessions(Pawn p, Map map, IntVec3 pos, Caravan caravan)
    {
        NarehateUtils.UnequipAll(p);
        if (map != null)
        {
            p.inventory.DropAllNearPawn(pos, forbid: true);
        }
        else if (caravan != null)
        {
            CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(p, caravan.PawnsListForReading);
        }
        else if (p.holdingOwner != null)
        {
            p.inventory.innerContainer.TryTransferAllToContainer(p.holdingOwner);
        }
    }

    // Lord：按主动离开结算，避免销毁时按 Vanished 结算
    private static void LeaveLord(Pawn p) => p.GetLord()?.Notify_PawnLost(p, PawnLostCondition.LeftVoluntarily);

    // 远行队：沿用原版成员损失处理；仍有殖民者时只移出成员，最后一名殖民者离开时放弃物资并解散远行队
    private static void LeaveCaravan(Pawn p, Caravan caravan) => caravan.Notify_MemberDied(p);

    // 销毁实例：未生成时先退出容器，部分容器销毁内容物时不会自动移除；已生成时 holdingOwner 是地图，由 DeSpawn 处理
    private static void DestroyInstance(Pawn p)
    {
        if (!p.Spawned) p.holdingOwner?.Remove(p);
        p.Destroy();
        if (p.Faction != Faction.OfPlayer) return;
        BillUtility.Notify_ColonistUnavailable(p);
        Find.ColonistBar.MarkColonistsDirty();
    }

    private static void AddThoughtToAllColonists(Map map, ThoughtDef thoughtDef)
    {
        if (map == null || thoughtDef == null)
        {
            if (ManosabaMod.Settings.DebugMode) Log.Warning("[Manosaba] Map or ThoughtDef is null. (Utils.YukiGeneralUtils.AddThoughtToAllColonists)");
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