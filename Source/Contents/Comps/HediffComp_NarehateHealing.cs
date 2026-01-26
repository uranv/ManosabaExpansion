using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_NarehateHealing : HediffCompProperties
{
    public bool applyPostMultiplier = false;
    public bool applyStatusSwitch = false;
    public int overrideHealFactor = -1;
    public int overrideBloodFactor = -1;
    public int overrideRegenTick = -1;
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
            if (!Props.applyStatusSwitch) return Props.applyPostMultiplier;
            return !CachedHumanDummy.cachedIsNarehate;
        }
    }
    private bool IsOverrideHealFactor => Props.overrideHealFactor > 0;
    private bool IsOverrideBloodFactor => Props.overrideBloodFactor > 0;
    private bool IsOverrideRegenTickInterval => Props.overrideRegenTick > 0;

    private float EffectiveHealFactor
    {
        get
        {
            if (IsOverrideHealFactor) return Props.overrideHealFactor;
            var multiplier = ApplyPostMultiplier ? ManosabaMod.Settings.postHealMultiplier * 10 : 1;
            var divisor = ManosabaMod.Settings.isNarehateDownedDivisor && Pawn.Downed ? ManosabaMod.Settings.narehateDownedDivisor : 1;
            return ManosabaMod.Settings.narehateHealFactor * multiplier / divisor;
        }
    }
    private float EffectiveBloodHealFactor 
    {
        get
        {
            if (IsOverrideBloodFactor) return Props.overrideBloodFactor;
            var multiplier = ApplyPostMultiplier ? ManosabaMod.Settings.postHealMultiplier * 10 : 1;
            var divisor = ManosabaMod.Settings.isNarehateDownedDivisor && Pawn.Downed ? ManosabaMod.Settings.narehateDownedDivisor : 1;
            return ManosabaMod.Settings.narehateBloodHealFactor * multiplier / divisor;
        }
    }

    private int HealTickInterval => (IsOverrideHealFactor || !ApplyPostMultiplier) ? 60 : 600;
    private int RegenTickInterval
    {
        get
        {
            if (IsOverrideRegenTickInterval) return Props.overrideRegenTick;
            var baseInterval = ApplyPostMultiplier ? 60000 : 600;
            var multiplier = ManosabaMod.Settings.isNarehateDownedDivisor && Pawn.Downed ? ManosabaMod.Settings.narehateDownedDivisor : 1;
            return Mathf.RoundToInt(baseInterval * multiplier);
        }
    }
    
    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);

        if (Props.applyStatusSwitch && !CachedHumanDummy.cachedIsFinished) return;
        
        if (Pawn.IsHashIntervalTick(HealTickInterval))
        {
            if (ApplyPostMultiplier && !ManosabaMod.Settings.postAllowHeal) return;
            Utils.HealingUtils.TryHeal(Pawn, EffectiveHealFactor);
            if (ManosabaMod.Settings.isNarehateBloodHeal) Utils.HealingUtils.TryBloodLoss(Pawn, EffectiveBloodHealFactor);
        }
        if (Pawn.IsHashIntervalTick(RegenTickInterval))
        {
            Utils.HealingUtils.TryRegenerate(Pawn, 1);
        }
    }

    public override string CompTipStringExtra
    {
        get
        {
            var result = string.Empty;
            // 若检查进度，魔女化前不恢复不需要显示
            if (Props.applyStatusSwitch && !CachedHumanDummy.cachedIsFinished) return result;
            // 若使用后系数，若系数为 0 禁用不需要显示
            if (ApplyPostMultiplier && !ManosabaMod.Settings.postAllowHeal) return result;
            // 生成说明
            var healPerDay = EffectiveHealFactor;
            var bloodHealPerDay = EffectiveBloodHealFactor;
            if (ApplyPostMultiplier)
            {
                healPerDay *= 10f;
                bloodHealPerDay *= 10f;
            }
            var stringHeal = "HediffComp_NarehateHealing_TipHeal".Translate(Mathf.Round(healPerDay));
            var stringBlood = "HediffComp_NarehateHealing_TipBlood".Translate(Mathf.Round(bloodHealPerDay));
            result += stringHeal;
            if (ManosabaMod.Settings.isNarehateBloodHeal) result += "\n" + stringBlood;
            return result;
        }
    }
}