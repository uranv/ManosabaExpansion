using RimWorld;
using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Utils;
/// <summary>
/// Static methods for healing related
/// </summary>
public static class HealingUtils
{
    /// <summary>
    /// Try to regenerate {count} number of missing part per call
    /// </summary>
    public static void TryRegenerate(Pawn pawn, int count = 1)
    {
        var hasThrown = false;
        var missingParts = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
        if (missingParts.NullOrEmpty()) return;
        var partsToRestore = missingParts.Take(count).ToList();
        foreach (var part in partsToRestore)
        {
            pawn.health.RestorePart(part.Part);
            var partName = part.Part.def.label.CapitalizeFirst();
            Messages.Message("Manosaba_Message_PartRegen".Translate(pawn, partName), null, MessageTypeDefOf.NeutralEvent, false);
            if (pawn.Map == null) continue;
            //Messages.Message("Manosaba_Message_PartRegen".Translate(pawn, partName), null, MessageTypeDefOf.NeutralEvent, false);
            //FleckMaker.ThrowMicroSparks(pawn.DrawPos, pawn.Map);
            if (!hasThrown) FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.HealingCross, velocitySpeed: 0.5f);
            hasThrown = true;
        }
    }
    /// <summary>
    /// Try to recover all wounds by {narehateHealFactor/1000} hp per call
    /// - 默认每 60 ticks 调用一次, 对应每日 1000 次愈合对应 narehateHealFactor
    /// </summary>
    public static void TryHeal(Pawn pawn,float factorHealing = 0f)
    {
        var hasThrown = false;
        var hediffs = pawn.health.hediffSet.hediffs;
        for (var i = hediffs.Count - 1; i >= 0; i--)
        {
            if (hediffs[i] is Hediff_Injury injury)
            {
                injury.Heal(factorHealing/1000f);
                if (pawn.Map != null && !hasThrown)
                {
                    //FleckMaker.ThrowMicroSparks(pawn.DrawPos, pawn.Map);
                    FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.HealingCross, velocitySpeed: 0.5f);
                    hasThrown = true;
                }
            }
        }
    }
    /// <summary>
    /// Try to heal blood loss by {narehateBloodHealFactor/1000}% per call
    /// - 默认每 60 ticks 调用一次, 对应每日 1000 次愈合
    /// - narehateBloodHealFactor 为百分比系数，所以整体归一化系数为 1000*100
    /// </summary>
    public static void TryBloodLoss(Pawn pawn,float factorBloodHealing = 0f)
    {
        var bloodLoss = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
        if (bloodLoss == null) return;
        var effectiveBloodHeal = Mathf.Max(factorBloodHealing/100000f, 0.000001f);
        bloodLoss.Severity -= Mathf.Min(effectiveBloodHeal, bloodLoss.Severity);
    }
}