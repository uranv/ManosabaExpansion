using HarmonyLib;
using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Patches;

// 征召时自动添加 hediff
[HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted")]
public static class Patch_Pawn_DraftController_Set_Drafted
{
    public static void Postfix(Pawn_DraftController __instance)
    {
        if (__instance is not { pawn: { } p })
        {
            return;
        }
        if (p is { Dead: true })
        {
            return;
        }
        if (p.health?.hediffSet is null)
        {
            return;
        }
        if (p is not { IsMutant: true, mutant.Def.defName: "UmMutantNarehate" })
        {
            return;
        }
        var targetHediff = ModDefOf.UmHediffMutantDraft;
        if (targetHediff == null)
        {
            Log.WarningOnce("[Manosaba] Drafted hediffDef \"UmHediffMutantDraft\" not found (Patch_Pawn_DraftController_Set_Drafted)",
                Gen.HashCombine(p.thingIDNumber, "Patch_Pawn_DraftController_Set_Drafted"));
            return;
        }
        if (__instance.Drafted)
        {
            if (!p.health.hediffSet.HasHediff(targetHediff))
            {
                p.health.AddHediff(targetHediff);
            }
        }
        else
        {
            var hediff = p.health.hediffSet.GetFirstHediffOfDef(targetHediff);
            if (hediff != null)
            {
                p.health.RemoveHediff(hediff);
            }
        }

    }
}