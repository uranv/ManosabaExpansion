using HarmonyLib;
using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Patches;
// 禁用魔女残骸器官收割，禁用月代雪器官收割
[HarmonyPatch(typeof(MedicalRecipesUtility), "IsCleanAndDroppable")]
public static class Patch_MedicalRecipesUtility_IsCleanAndDroppable
{
    public static bool Prefix(Pawn pawn, BodyPartRecord part, ref bool __result)
    {
        if (pawn == null || pawn.Dead) return true;
        if (!pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffNarehate)) return true;
        if (!pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffYukiDummy)) return true;
        __result = false;
        return false;
    }
}