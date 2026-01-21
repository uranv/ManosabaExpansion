using HarmonyLib;
using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Patches;

// Patch: 修改魔女残骸 Mutant 的负重
[HarmonyPatch(typeof(MassUtility), "Capacity")]
public static class Patch_MassUtility_Capacity
{
    public static void Postfix(Pawn p, ref float __result)
    {
        if (p is { IsMutant: true } && p.mutant.Def.defName == "UmMutantNarehate")
        {
            if (__result <= 0.1f)
            {
                __result = 35f * p.BodySize;
            }
            if (Find.Scenario != null)
            {
                __result *= Find.Scenario.GetStatFactor(StatDefOf.CarryingCapacity);
            }
        }
    }
}