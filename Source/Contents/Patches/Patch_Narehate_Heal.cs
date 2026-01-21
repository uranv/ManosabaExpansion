using System.Diagnostics;
using HarmonyLib;
using Verse;

namespace UranvManosaba.Contents.Patches;

// Patch: 拦截非意图的基因治愈魔女残骸 Hediff
[HarmonyPatch(typeof(Pawn_HealthTracker), "RemoveHediff")]
public static class Patch_ConditionalCure
{
    static bool Prepare()
    {
        return ModsConfig.BiotechActive;;
    }
    
    public static bool Prefix(Pawn_HealthTracker __instance, Hediff hediff)
    {

        if (hediff == null || hediff.def != ModDefOf.UmHediffNarehate) return true;
        var stackTrace = new StackTrace();
        for (int i = 0; i < stackTrace.FrameCount; i++)
        {
            var method = stackTrace.GetFrame(i).GetMethod();
            if (method == null) continue;
            var declaringType = method.DeclaringType;
            if (declaringType == null) continue;
            if (!typeof(Gene).IsAssignableFrom(declaringType) && !declaringType.Name.Contains("Gene")) continue;
            if (ManosabaMod.Settings.debugMode) Log.Warning($"[Manosaba] Blocked <GeneDef> {declaringType.Name} trying to remove <Hediff> UmHediffNarehate (Patches.Patch_ConditionalCure)");
            return false;
        }
        return true;
    }
}