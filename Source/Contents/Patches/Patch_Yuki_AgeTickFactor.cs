using HarmonyLib;
using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Patches;

// 月代雪15.5岁后不衰老
[HarmonyPatch(typeof(Pawn_GeneTracker), "BiologicalAgeTickFactor", MethodType.Getter)]
public static class Patch_GeneTracker_BiologicalAgeTickFactor
{
    public static void Postfix(Pawn_GeneTracker __instance, ref float __result)
    {
        if (__instance is not { pawn: { } pawn })
        {
            return;
        }
        var yukiDummy = ModDefOf.UmHediffYukiDummy;
        if (yukiDummy == null)
        {
            Log.ErrorOnce("[Manosaba] HediffDef \"UmHediffYukiDummy\" not found (Patch_GeneTracker_BiologicalAgeTickFactor)",
                Gen.HashCombine(pawn.thingIDNumber,"Patch_GeneTracker_BiologicalAgeTickFactor")); 
            return;
        }
        if (pawn.health?.hediffSet == null ||
            !pawn.health.hediffSet.HasHediff(yukiDummy))
        {
            return;
        }
        if (pawn.ageTracker?.AgeBiologicalYearsFloat >= 15.5f)
        {
            __result = 0f;
        }
    }
}