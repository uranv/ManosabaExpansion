using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_NarehateHealingReveal : HediffCompProperties
{
    public bool applyPostMultiplier = false;
    public bool isOverride = false;
    public float overrideHealFactor = 0;
    public float overrideBloodFactor = 0;
    public float overrideRegenFactor = 0;
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
            if (Props.isOverride)
            {
                return Props.overrideHealFactor;
            }
            var factor = ManosabaMod.Settings.narehateHealFactor;
            if (Props.applyPostMultiplier)
            {
                factor *= ManosabaMod.Settings.postHealMultiplier;
            }
            else if (ManosabaMod.Settings.isNarehateDownedDivisor && Pawn.Downed)
            {
                factor /= ManosabaMod.Settings.narehateDownedDivisor;
            }
            return factor;
        }
    }
    private float EffectiveBloodHealPerDay 
    {
        get
        {
            if (Props.isOverride)
            {
                return Props.overrideBloodFactor;
            }
            var factor = ManosabaMod.Settings.narehateBloodHealFactor;
            if (Props.applyPostMultiplier)
            {
                factor *= ManosabaMod.Settings.postHealMultiplier;
            }
            else if (ManosabaMod.Settings.isNarehateDownedDivisor && Pawn.Downed)
            {
                factor /= ManosabaMod.Settings.narehateDownedDivisor;
            }
            return factor;
        }
    }
    
    private float RegenTickExpected
    {
        get
        {
            if (Props.isOverride)
            {
                return 60f / Props.overrideRegenFactor;
            }
            var tick = 600f;
            if (Props.applyPostMultiplier)
            {
                tick *= 100f;
            }
            else if (ManosabaMod.Settings.isNarehateDownedDivisor && Pawn.Downed)
            {
                tick *= ManosabaMod.Settings.narehateDownedDivisor;
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
            if (Props.applyPostMultiplier && !ManosabaMod.Settings.postAllowHeal) return result;
            // 生成说明
            string stringHeal;
            string stringBlood;
            if (Props.isOverride)
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
            if (ManosabaMod.Settings.isNarehateBloodHeal) result += "\n" + stringBlood;
            result += "\n" + RegenTip;
            return result;
        }
    }
}