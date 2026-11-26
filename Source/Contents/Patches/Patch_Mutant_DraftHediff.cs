using HarmonyLib;
using RimWorld;
using UranvManosaba.Contents.Comps;

namespace UranvManosaba.Contents.Patches;

// 征召时自动添加 hediff
[HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted")]
public static class Patch_Pawn_DraftController_Set_Drafted
{
    public static void Postfix(Pawn_DraftController __instance)
    {
        HediffComp_MutantDraft.Sync(__instance.pawn);
    }
}
