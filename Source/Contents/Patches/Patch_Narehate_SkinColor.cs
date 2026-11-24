using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Patches;

[HarmonyPatch(typeof(Pawn_StoryTracker), nameof(Pawn_StoryTracker.SkinColor), MethodType.Getter)]
public static class Patch_NarehateSkinColor
{
    public static void Postfix(Pawn ___pawn, ref Color __result)
    {
        if (___pawn.health?.hediffSet.HasHediff(ModDefOf.UmHediffNarehate) == true ||
            (___pawn.mutant is { } mutant && mutant.Def == ModDefOf.UmMutantNarehate))
        {
            __result *= new Color(0.22f, 0.23f, 0.27f, 1f);
        }
    }
}
