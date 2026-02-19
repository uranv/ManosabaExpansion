using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace UranvManosaba.Contents.Patches;

// Patch: 阻止复活存在 B水 Hediff 的尸体
// 拦截 JobDriver_Resurrect.Resurrect 方法
[HarmonyPatch(typeof(JobDriver_Resurrect), "Resurrect")]
public static class Patch_JobDriver_Resurrect_Manual
{
    public static bool Prefix(JobDriver_Resurrect __instance)
    {
        if (__instance?.job?.GetTarget(TargetIndex.A).Thing is not Corpse { InnerPawn: not null } corpse)
        {
            return true;
        }
        if (corpse.InnerPawn?.health?.hediffSet == null ||
            !corpse.InnerPawn.health.hediffSet.HasHediff(ModDefOf.UmHediffTredecim))
        {
            return true;
        }
        if (ManosabaMod.Settings.debugMode)
        {
            Log.Warning($"[Manosaba] Block resurrection for {corpse.InnerPawn.LabelShort} (Patch_JobDriver_Resurrect_Manual)");
        }
        
        var text = "Manosaba_Message_NoResurrect".Translate(corpse.InnerPawn.LabelShort);
        Messages.Message(text, corpse.InnerPawn, MessageTypeDefOf.RejectInput);

        return false;
    }
}