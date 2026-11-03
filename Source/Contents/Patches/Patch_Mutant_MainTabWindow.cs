using HarmonyLib;
using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Patches;

// 非 subhuman 的残骸已由原版纳入工作与方案面板；仅在设置关闭时从方案面板隐藏。
[HarmonyPatch(typeof(MainTabWindow_Assign), "Pawns", MethodType.Getter)]
public static class Patch_MainTabWindow_Assign_Pawns
{
    private static bool Prepare() => !ManosabaMod.Settings.AllowMutantAssignTab;

    public static void Postfix(ref IEnumerable<Pawn> __result)
    {
        __result = __result.Where(p => p is not { IsMutant: true, mutant.Def.defName: "UmMutantNarehate" });
    }
}
