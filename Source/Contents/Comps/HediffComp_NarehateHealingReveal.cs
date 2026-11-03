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

    private float EffectiveHealPerDay
    {
        get
        {
            if (Props.IsOverride)
            {
                return Props.OverrideHealFactor;
            }
            var factor = ManosabaMod.Settings.NarehateHealFactor;
            if (Props.ApplyPostMultiplier)
            {
                factor *= ManosabaMod.Settings.PostHealMultiplier;
            }
            else if (ManosabaMod.Settings.IsNarehateDownedDivisor && Pawn.Downed)
            {
                factor /= ManosabaMod.Settings.NarehateDownedDivisor;
            }
            return factor;
        }
    }
    private float EffectiveBloodHealPerDay
    {
        get
        {
            if (Props.IsOverride)
            {
                return Props.OverrideBloodFactor;
            }
            var factor = ManosabaMod.Settings.NarehateBloodHealFactor;
            if (Props.ApplyPostMultiplier)
            {
                factor *= ManosabaMod.Settings.PostHealMultiplier;
            }
            else if (ManosabaMod.Settings.IsNarehateDownedDivisor && Pawn.Downed)
            {
                factor /= ManosabaMod.Settings.NarehateDownedDivisor;
            }
            return factor;
        }
    }

    private float RegenTickExpected
    {
        get
        {
            if (Props.IsOverride)
            {
                return 60f / Props.OverrideRegenFactor;
            }
            var tick = 600f;
            if (Props.ApplyPostMultiplier)
            {
                tick *= 100f;
            }
            else if (ManosabaMod.Settings.IsNarehateDownedDivisor && Pawn.Downed)
            {
                tick *= ManosabaMod.Settings.NarehateDownedDivisor;
            }
            return tick;
        }
    }

    private string RegenTip
    {
        get
        {
            float regenCount;
            string tipRegenNumber;
            var regenTipUnit1 = "HediffComp_NarehateHealing_TipRegenUnit1".Translate();
            var regenTipUnit2 = "HediffComp_NarehateHealing_TipRegenUnit2".Translate();
            var regenTipUnit3 = "HediffComp_NarehateHealing_TipRegenUnit3".Translate();
            var regenTipUnit4 = "HediffComp_NarehateHealing_TipRegenUnit4".Translate();
            var regenTipUnit5 = "HediffComp_NarehateHealing_TipRegenUnit5".Translate();
            switch(RegenTickExpected)
            {
                // 显示器官自愈：x日/处
                case >60000:
                    regenCount = Mathf.Round(RegenTickExpected / 60000);
                    tipRegenNumber = regenCount.ToString("F0") + regenTipUnit1;
                    break;
                // 显示器官自愈：x处/日
                case >2500:
                    regenCount = Mathf.Round(60000 / RegenTickExpected);
                    tipRegenNumber = regenCount.ToString("F0") + regenTipUnit2;
                    break;
                // 显示器官自愈：x处/小时
                case >300:
                    regenCount = Mathf.Round(2500 / RegenTickExpected);
                    tipRegenNumber = regenCount.ToString("F0") + regenTipUnit3;
                    break;
                // 显示器官自愈：x秒/处
                case >60:
                    regenCount = Mathf.Round(RegenTickExpected / 60);
                    tipRegenNumber = regenCount.ToString("F0") + regenTipUnit4;
                    break;
                // 显示器官自愈：x处/秒
                case >0:
                    regenCount = Mathf.Round(60 / RegenTickExpected);
                    tipRegenNumber = regenCount.ToString("F0") + regenTipUnit5;
                    break;
                // 错误
                default:
                    tipRegenNumber = "ERR";
                    break;
            }
            var stringRegen = "HediffComp_NarehateHealing_TipRegen".Translate() + tipRegenNumber;
            return stringRegen;
        }
    }

    public override string CompTipStringExtra
    {
        get
        {
            var result = string.Empty;
            // 若使用后系数，若系数为 0 禁用不需要显示
            if (Props.ApplyPostMultiplier && !ManosabaMod.Settings.PostAllowHeal) return result;
            // 生成说明
            string stringHeal;
            string stringBlood;
            if (Props.IsOverride)
            {
                stringHeal = "HediffComp_NarehateHealing_TipHealOverride".Translate((EffectiveHealPerDay/1000).ToString("F1"));
                stringBlood = "HediffComp_NarehateHealing_TipBloodOverride".Translate((EffectiveBloodHealPerDay/1000).ToString("F1"));
            }
            else
            {
                stringHeal = "HediffComp_NarehateHealing_TipHeal".Translate(Mathf.Round(EffectiveHealPerDay));
                stringBlood = "HediffComp_NarehateHealing_TipBlood".Translate(Mathf.Round(EffectiveBloodHealPerDay));
            }
            result += stringHeal;
            if (ManosabaMod.Settings.IsNarehateBloodHeal) result += "\n" + stringBlood;
            result += "\n" + RegenTip;
            return result;
        }
    }
}