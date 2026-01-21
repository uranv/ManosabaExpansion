using HarmonyLib;
using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Patches;

// 月代雪tab允许绘制恋爱选项
[HarmonyPatch(typeof(SocialCardUtility), "CanDrawTryRomance")]
public static class Patch_CanDrawTryRomance_Prefix
{
    public static bool Prefix(Pawn pawn, ref bool __result)
    {
        if (__result) return true;
        var thisIsYuki = pawn.kindDef == ModDefOf.UmPawnKindYukiColonist ||
                         pawn.kindDef == ModDefOf.UmPawnKindYukiVisitor;
        if (!thisIsYuki) return true;
        if (!pawn.Spawned || pawn.Dead) return true;
        __result = true;
        return false;
    }
}