using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Patches;

// Patch: 允许残骸使用工作台 (炉灶、锻造台等)
[HarmonyPatch(typeof(Bill), "PawnAllowedToStartAnew")]
public static class Patch_Bill_PawnAllowedToStartAnew
{
    public static void Postfix(Bill __instance, Pawn p, ref bool __result)
    {
        if (__result) return;

        if (p.IsMutant && p.mutant.Def.defName == "UmMutantNarehate")
        {
            if (__instance.PawnRestriction != null && __instance.PawnRestriction != p) return;
            if (__instance.suspended) return;
            __result = true;
        }
    }
}
    
// Patch: 允许残骸互动工作台 (炉灶、锻造台等)
// 拦截对 pawn.IsMutant 的调用,把 pawn.IsMutant 替换为 IsMutantButNotNarehate(pawn)
[HarmonyPatch(typeof(FloatMenuOptionProvider), "SelectedPawnValid")]
public static class Patch_FloatMenuOptionProvider_SelectedPawnValid
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var targetMethod = AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.IsMutant));
        var replacementMethod = AccessTools.Method(typeof(Patch_FloatMenuOptionProvider_SelectedPawnValid),
            nameof(IsMutantButNotNarehate));

        foreach (var code in instructions)
        {
            if (code.Calls(targetMethod))
            {
                code.opcode = OpCodes.Call;
                code.operand = replacementMethod;
            }
            yield return code;
        }
    }

    public static bool IsMutantButNotNarehate(Pawn p)
    {
        if (p.IsMutant && p.mutant.Def.defName == "UmMutantNarehate")
        {
            return false; 
        }
        return p.IsMutant;
    }
}