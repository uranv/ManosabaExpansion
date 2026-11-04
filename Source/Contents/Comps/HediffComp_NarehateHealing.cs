using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_NarehateHealing : HediffCompProperties
{
    public bool ApplyPostMultiplier = false;
    public bool ApplyStatusSwitch = false;
    public bool IsOverride = false;
    public float OverrideHealFactor = 0;
    public float OverrideBloodFactor = 0;
    public float OverrideRegenFactor = 0;
    public HediffCompProperties_NarehateHealing()
    {
        compClass = typeof(HediffComp_NarehateHealing);
    }
}

public class HediffComp_NarehateHealing : HediffComp
{
    private HediffCompProperties_NarehateHealing Props => (HediffCompProperties_NarehateHealing)props;

    private HediffComp_HumanDummy _cachedHumanDummy;
    private HediffComp_HumanDummy CachedHumanDummy => _cachedHumanDummy ??= parent.TryGetComp<HediffComp_HumanDummy>();
    private bool ApplyPostMultiplier
    {
        get
        {
            if (!Props.ApplyStatusSwitch) return Props.ApplyPostMultiplier;
            return !CachedHumanDummy.CachedIsNarehate;
        }
    }
    private float EffectiveHealFactor => Utils.HealingUtils.GetNarehateRecoveryFactor(
        Pawn, ManosabaMod.Settings.NarehateHealFactor, ApplyPostMultiplier,
        Props.IsOverride, Props.OverrideHealFactor, nameof(Props.OverrideHealFactor));
    private float EffectiveBloodHealFactor => Utils.HealingUtils.GetNarehateRecoveryFactor(
        Pawn, ManosabaMod.Settings.NarehateBloodHealFactor, ApplyPostMultiplier,
        Props.IsOverride, Props.OverrideBloodFactor, nameof(Props.OverrideBloodFactor));
    private float RegenChance => Utils.HealingUtils.GetNarehateRegenChance(
        Pawn, ApplyPostMultiplier, Props.IsOverride, Props.OverrideRegenFactor);
    private int TickInterval => Utils.HealingUtils.GetNarehateTickInterval(Props.IsOverride, ApplyPostMultiplier);

    public override void CopyFrom(HediffComp other)
    {
        base.CopyFrom(other);
        _cachedHumanDummy = null;
    }

    public override void CompPostTickInterval(ref float severityAdjustment, int delta)
    {
        base.CompPostTickInterval(ref severityAdjustment, delta);
        // 无效 MutantDummy 等待移除时，以及 HumanDummy 错挂在 mutant 上时，不执行治疗。
        if (parent.ShouldRemove || (Props.ApplyStatusSwitch && Pawn.IsMutant)) return;

        if (Pawn.IsHashIntervalTick(TickInterval, delta))
        {
            if (Props.ApplyStatusSwitch && !CachedHumanDummy.CachedIsFinished) return;
            if (ApplyPostMultiplier && !ManosabaMod.Settings.PostAllowHeal) return;

            Utils.HealingUtils.TryHeal(Pawn, EffectiveHealFactor);

            if (ManosabaMod.Settings.IsNarehateBloodHeal)
            {
                Utils.HealingUtils.TryBloodLoss(Pawn, EffectiveBloodHealFactor);
            }

            if (Rand.Value <= RegenChance)
            {
                Utils.HealingUtils.TryRegenerate(Pawn, 1);
            }
        }
    }

    public override string CompDebugString()
    {
        if (Props.ApplyStatusSwitch && !CachedHumanDummy.CachedIsFinished) return string.Empty;
        if (ApplyPostMultiplier && !ManosabaMod.Settings.PostAllowHeal) return string.Empty;

        var result = "HediffComp_NarehateHealing_TipHeal".Translate(Mathf.Round(
            Utils.HealingUtils.GetNarehateDailyRate(EffectiveHealFactor, TickInterval))).ToString();
        if (ManosabaMod.Settings.IsNarehateBloodHeal)
        {
            result += "\n" + "HediffComp_NarehateHealing_TipBlood".Translate(Mathf.Round(
                Utils.HealingUtils.GetNarehateDailyRate(EffectiveBloodHealFactor, TickInterval)));
        }
        return result + "\n" + Utils.HealingUtils.GetNarehateRegenTip(Pawn, RegenChance, TickInterval);
    }
}
