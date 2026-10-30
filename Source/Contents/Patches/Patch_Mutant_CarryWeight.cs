using HarmonyLib;
using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Patches;

// Patch: 将剧本中的负重倍率应用于魔女残骸 Mutant
[HarmonyPatch(typeof(MassUtility), "Capacity")]
public static class Patch_MassUtility_Capacity
{
    public static void Postfix(Pawn p, ref float __result)
    {
        if (p is not { IsMutant: true, mutant.Def.defName: "UmMutantNarehate" })
        {
            return;
        }
        if (Find.Scenario is not null)
        {
            __result *= Find.Scenario.GetStatFactor(StatDefOf.CarryingCapacity);
        }
    }
}