using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace UranvManosaba.Contents.PsychicRituals;

public class PsychicRitualToil_NarehateTrans : PsychicRitualToil
{
    private PsychicRitualRoleDef _targetRole;
    private SimpleCurve _qualityCurve;
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref _targetRole, "_targetRole");
        Scribe_Values.Look(ref _qualityCurve, "_qualityCurve");
    }
    public PsychicRitualToil_NarehateTrans(PsychicRitualRoleDef targetRole, SimpleCurve qualityCurve)
    {
        _targetRole = targetRole;
        _qualityCurve = qualityCurve;
    }

    public override void Start(Verse.AI.Group.PsychicRitual psychicRitual, PsychicRitualGraph parent)
    {
        base.Start(psychicRitual, parent);
        var targetPawn = psychicRitual.assignments.FirstAssignedPawn(_targetRole);
        psychicRitual.ReleaseAllPawnsAndBuildings();
            
        if (targetPawn?.health?.hediffSet == null) return;
            
        var quality = psychicRitual.power; // 0.0 到 1.0
        var targetSeverity = _qualityCurve.Evaluate(quality);
        var narehateDef = ModDefOf.UmMutantNarehate;//DefDatabase<MutantDef>.GetNamed("UmMutantNarehate");
        var alreadyMutant = targetPawn.IsMutant && targetPawn.mutant.Def == narehateDef;
        // 转化为 mutant
        if (!alreadyMutant)
        {
            MutantUtility.SetPawnAsMutantInstantly(targetPawn, narehateDef);
            Utils.NarehateUtils.RefreshPawnGraphics(targetPawn);
            // 卸下装备
            Utils.NarehateUtils.UnequipAll(targetPawn);
            // 初始化工作与思维
            targetPawn.workSettings?.EnableAndInitialize();
            targetPawn.jobs.StopAll();
            targetPawn.mindState.Reset(true, true);
            // 强制玩家派系
            if (targetPawn.Faction != Faction.OfPlayer)
            {
                targetPawn.SetFaction(Faction.OfPlayer);
            }
        }
        // 调整 pawn 属性
        MutantStyleChanger(targetPawn);
        // 检查已有 hediffs
        if (targetPawn.health?.hediffSet == null) return; 
        var dummyHumanDef = ModDefOf.UmHediffHumanDummy;
        var hanmajyoDef = ModDefOf.UmHediffNarehateHealed;
        var visibleDef = ModDefOf.UmHediffHumanVisible; 
        var dummyMutantDef = ModDefOf.UmHediffMutantDummy;
        if (targetPawn.health?.hediffSet != null)
        {
            var hm = targetPawn.health.hediffSet.GetFirstHediffOfDef(hanmajyoDef); 
            if (hm != null) targetPawn.health.RemoveHediff(hm);
            var hv = targetPawn.health.hediffSet.GetFirstHediffOfDef(visibleDef); 
            if (hv != null) targetPawn.health.RemoveHediff(hv);
            var hd = targetPawn.health.hediffSet.GetFirstHediffOfDef(dummyHumanDef); 
            if (hd != null) targetPawn.health.RemoveHediff(hd); 
            if (!targetPawn.health.hediffSet.HasHediff(dummyMutantDef)) targetPawn.health.AddHediff(dummyMutantDef);
        }
        // 更新 UmHediffMutantCountdown 的严重度
        var countdownDef = ModDefOf.UmHediffMutantCountdown;
        var countdownHediff = targetPawn.health?.hediffSet.GetFirstHediffOfDef(countdownDef) ?? targetPawn.health?.AddHediff(countdownDef);
        if (countdownHediff != null) countdownHediff.Severity = Mathf.Min(countdownHediff.Severity + targetSeverity / 100f, 1f);
    }


    private static void MutantStyleChanger(Pawn p)
    {
        try
        {
            p.story.bodyType = BodyTypeDefOf.Thin;
            p.story.hairDef = HairDefOf.Bald;
            p.story.furDef = null;
            p.story.skinColorOverride = new Color32(53, 54, 59, 255);
            if (p.style != null)
            {
                p.style.FaceTattoo = TattooDefOf.NoTattoo_Face;
                p.style.BodyTattoo = TattooDefOf.NoTattoo_Body;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[Manosaba] Skipped unsafe pawn's style change:{ex} (PsychicRituals.PsychicRitualToil_NarehateTrans.MutantStyleChanger)");
        }
        p.Drawer.renderer.SetAllGraphicsDirty();
    }
}