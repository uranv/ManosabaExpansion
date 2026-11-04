using RimWorld;
using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Utils;
/// <summary>
/// Static methods for healing related
/// </summary>
public static class HealingUtils
{
    public static float GetNarehateRecoveryFactor(Pawn pawn, float configuredFactor,
        bool applyPostMultiplier, bool isOverride, float overrideFactor, string overrideName)
    {
        if (isOverride)
        {
            if (overrideFactor <= 0f)
            {
                Log.WarningOnce($"[Manosaba] {overrideName} must be positive. Using a recovery factor of 1.",
                    Gen.HashCombine(pawn.thingIDNumber, "HealingUtils.RecoveryFactor." + overrideName));
                return 1f;
            }
            return overrideFactor;
        }
        if (applyPostMultiplier) return configuredFactor * (ManosabaMod.Settings.PostHealMultiplier * 10f);
        if (ManosabaMod.Settings.IsNarehateDownedDivisor && pawn.Downed)
            return configuredFactor / ManosabaMod.Settings.NarehateDownedDivisor;
        return configuredFactor;
    }

    public static float GetNarehateRegenChance(Pawn pawn, bool applyPostMultiplier,
        bool isOverride, float overrideFactor)
    {
        if (isOverride)
        {
            if (overrideFactor is <= 0f or > 1f)
            {
                Log.WarningOnce($"[Manosaba] OverrideRegenFactor is {overrideFactor}. Regeneration probability is clamped to [0, 1].",
                    Gen.HashCombine(pawn.thingIDNumber, "HealingUtils.RegenChance"));
            }
            return Mathf.Max(0f, Mathf.Min(overrideFactor, 1f));
        }
        if (applyPostMultiplier) return 0.1f / 10f;
        if (ManosabaMod.Settings.IsNarehateDownedDivisor && pawn.Downed)
            return 0.1f / ManosabaMod.Settings.NarehateDownedDivisor;
        return 0.1f;
    }

    public static int GetNarehateTickInterval(bool isOverride, bool applyPostMultiplier) =>
        isOverride || !applyPostMultiplier ? 60 : 600;

    public static float GetNarehateDailyRate(float factor, int tickInterval) => factor / (tickInterval / 60f);

    public static string GetNarehateRegenTip(Pawn pawn, float chance, int tickInterval)
    {
        var expectedTicks = tickInterval / chance;
        float count;
        string unit;
        switch (expectedTicks)
        {
            case > 60000f:
                count = expectedTicks / 60000f;
                unit = "HediffComp_NarehateHealing_TipRegenUnit1";
                break;
            case > 2500f:
                count = 60000f / expectedTicks;
                unit = "HediffComp_NarehateHealing_TipRegenUnit2";
                break;
            case > 600f:
                count = 2500f / expectedTicks;
                unit = "HediffComp_NarehateHealing_TipRegenUnit3";
                break;
            case > 60f:
                count = expectedTicks / 60f;
                unit = "HediffComp_NarehateHealing_TipRegenUnit4";
                break;
            case > 0f:
                count = 60f / expectedTicks;
                unit = "HediffComp_NarehateHealing_TipRegenUnit5";
                break;
            default:
                Log.ErrorOnce("[Manosaba] Expected regeneration interval must be positive.",
                    Gen.HashCombine(pawn.thingIDNumber, "HealingUtils.RegenTip"));
                return "HediffComp_NarehateHealing_TipRegen".Translate() + "ERR";
        }
        return "HediffComp_NarehateHealing_TipRegen".Translate() +
               Mathf.Round(count).ToString("F0") + unit.Translate();
    }

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