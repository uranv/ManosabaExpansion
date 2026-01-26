using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_NarehateHealingReveal : HediffCompProperties
{
    public bool applyPostMultiplier = false;

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
            if (!ManosabaMod.Settings.postAllowHeal) return -1f;
            var multiplier = Props.applyPostMultiplier ? ManosabaMod.Settings.postHealMultiplier : 1;
            var divisor = ManosabaMod.Settings.isNarehateDownedDivisor && Pawn.Downed ? ManosabaMod.Settings.narehateDownedDivisor : 1;
            return ManosabaMod.Settings.narehateHealFactor * multiplier / divisor;
        }
    }
    
    private float EffectiveBloodHealPerDay 
    {
        get
        {
            if (!ManosabaMod.Settings.isNarehateBloodHeal) return -1f;
            if (!ManosabaMod.Settings.postAllowHeal) return -1f;
            var multiplier = Props.applyPostMultiplier ? ManosabaMod.Settings.postHealMultiplier : 1;
            var divisor = ManosabaMod.Settings.isNarehateDownedDivisor && Pawn.Downed ? ManosabaMod.Settings.narehateDownedDivisor : 1;
            return ManosabaMod.Settings.narehateBloodHealFactor * multiplier / divisor;
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
            var healPerDay = EffectiveHealPerDay;
            var bloodHealPerDay = EffectiveBloodHealPerDay;
            var stringHeal = "HediffComp_NarehateHealing_TipHeal".Translate(Mathf.Round(healPerDay));
            var stringBlood = "HediffComp_NarehateHealing_TipBlood".Translate(Mathf.Round(bloodHealPerDay));
            result += stringHeal;
            if (ManosabaMod.Settings.isNarehateBloodHeal) result += "\n" + stringBlood;
            return result;
        }
    }
}