using HarmonyLib;
using RimWorld;

namespace UranvManosaba.Contents.Patches;

// 征召时自动添加 hediff
[HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted")]
public static class Patch_Pawn_DraftController_Set_Drafted
{
    public static void Postfix(Pawn_DraftController __instance)
    {
        var p = __instance.pawn;
        if (p?.health?.hediffSet == null || p.Dead) return;
        if (p is not { IsMutant: true }) return;
        if (p.mutant.Def.defName != "UmMutantNarehate") return;
            
        var targetHediff = ModDefOf.UmHediffMutantDraft;
        if (__instance.Drafted)
        {
            if (!p.health.hediffSet.HasHediff(targetHediff)) p.health.AddHediff(targetHediff);
        }
        else
        {
            var hediff = p.health.hediffSet.GetFirstHediffOfDef(targetHediff);
            if (hediff != null) p.health.RemoveHediff(hediff);
        }
    }
}