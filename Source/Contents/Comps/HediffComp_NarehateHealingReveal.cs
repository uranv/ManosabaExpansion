using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_NarehateHealingReveal : HediffCompProperties
{
    public bool ApplyPostMultiplier = false;
    public bool IsOverride = false;
    public float OverrideHealFactor = 0;
    public float OverrideBloodFactor = 0;
    public float OverrideRegenFactor = 0;
    public HediffCompProperties_NarehateHealingReveal()
    {
        compClass = typeof(HediffComp_NarehateHealingReveal);
    }
}

public class HediffComp_NarehateHealingReveal : HediffComp
{
    private HediffCompProperties_NarehateHealingReveal Props => (HediffCompProperties_NarehateHealingReveal)props;

    private float EffectiveHealFactor => Utils.HealingUtils.GetNarehateRecoveryFactor(
        Pawn, ManosabaMod.Settings.NarehateHealFactor, Props.ApplyPostMultiplier,
        Props.IsOverride, Props.OverrideHealFactor, nameof(Props.OverrideHealFactor));
    private float EffectiveBloodHealFactor => Utils.HealingUtils.GetNarehateRecoveryFactor(
        Pawn, ManosabaMod.Settings.NarehateBloodHealFactor, Props.ApplyPostMultiplier,
        Props.IsOverride, Props.OverrideBloodFactor, nameof(Props.OverrideBloodFactor));
    private float RegenChance => Utils.HealingUtils.GetNarehateRegenChance(
        Pawn, Props.ApplyPostMultiplier, Props.IsOverride, Props.OverrideRegenFactor);
    private int TickInterval => Utils.HealingUtils.GetNarehateTickInterval(Props.IsOverride, Props.ApplyPostMultiplier);

    public override string CompTipStringExtra
    {
        get
        {
            if (Props.ApplyPostMultiplier && !ManosabaMod.Settings.PostAllowHeal) return string.Empty;

            string heal;
            string blood;
            if (Props.IsOverride)
            {
                heal = "HediffComp_NarehateHealing_TipHealOverride".Translate((EffectiveHealFactor / 1000f).ToString("F1"));
                blood = "HediffComp_NarehateHealing_TipBloodOverride".Translate((EffectiveBloodHealFactor / 1000f).ToString("F1"));
            }
            else
            {
                heal = "HediffComp_NarehateHealing_TipHeal".Translate(Mathf.Round(
                    Utils.HealingUtils.GetNarehateDailyRate(EffectiveHealFactor, TickInterval)));
                blood = "HediffComp_NarehateHealing_TipBlood".Translate(Mathf.Round(
                    Utils.HealingUtils.GetNarehateDailyRate(EffectiveBloodHealFactor, TickInterval)));
            }
            if (ManosabaMod.Settings.IsNarehateBloodHeal) heal += "\n" + blood;
            return heal + "\n" + Utils.HealingUtils.GetNarehateRegenTip(Pawn, RegenChance, TickInterval);
        }
    }
}
