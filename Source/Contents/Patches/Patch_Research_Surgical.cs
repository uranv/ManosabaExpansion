using HarmonyLib;
using RimWorld;
using UranvManosaba.Contents.Utils;
using Verse;

namespace UranvManosaba.Contents.Patches;

// Patch: 修改异象 DLC 的外科手术, 使其可检查魔女因子 dummy Hediff
[HarmonyPatch(typeof(Recipe_SurgicalInspection), "ApplyOnPawn")]
public static class Patch_SurgicalInspection
{
    public static bool Prefix(Pawn pawn, Pawn billDoer)
    {
        if (pawn?.health?.hediffSet == null) return true;
        if (ModDefOf.UmResearchProjectDetect == null || !ModDefOf.UmResearchProjectDetect.IsFinished) return true;
        if (!pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffHumanDummy)) return true;
        // 若存在魔女因子 dummy Hediff
        // 发信通知
        SendInspectionLetter(pawn, billDoer);
        // 解锁科技
        ResearchUtils.UnlockResearchPrereqs();
        // 设置魔女因子为可见
        Comps.HediffComp_HumanDummy.SetDummyShouldDisplay(pawn);
        var visibleHediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(ModDefOf.UmHediffHumanVisible);
        if (visibleHediff == null)
        {
            pawn.health?.AddHediff(ModDefOf.UmHediffHumanVisible);
        }
        return false;
    }
    private static void SendInspectionLetter(Pawn patient, Pawn doctor)
    {
        var label = "Manosaba_SurgicalInspection_Label".Translate();
        var text = "Manosaba_SurgicalInspection_Text".Translate(
            patient.Named("PATIENT"), 
            doctor.Named("DOCTOR")
        );
        Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, patient);
    }
}