using HarmonyLib;
using RimWorld;

namespace UranvManosaba.Contents.Patches;

// 月代雪15.5岁后不衰老
[HarmonyPatch(typeof(Pawn_GeneTracker), "BiologicalAgeTickFactor", MethodType.Getter)]
public static class Patch_GeneTracker_BiologicalAgeTickFactor
{
    public static void Postfix(Pawn_GeneTracker __instance, ref float __result)
    {
        var pawn = __instance.pawn;
        if (pawn == null) return;
        var yukiDummy = ModDefOf.UmHediffYukiDummy;
        if ( pawn.health?.hediffSet?.HasHediff(yukiDummy) ?? true)
        {
            var ageYears = pawn.ageTracker.AgeBiologicalYearsFloat;
            if (ageYears >= 15.5f) __result = 0f;
        }
    }
}