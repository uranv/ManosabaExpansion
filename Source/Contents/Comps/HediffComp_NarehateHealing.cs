using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_NarehateHealing : HediffCompProperties
{
    public bool applyPostMultiplier = false;
    public bool applyStatusSwitch = false;
    public bool isOverride = false;
    public float overrideHealFactor = 0;
    public float overrideBloodFactor = 0;
    public float overrideRegenFactor = 0;
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
    private float EffectiveHealFactor
    {
        get
        {
            if (Props.isOverride)
            {
                if (Props.overrideHealFactor <= 0)
                {
                    var log = "[Manosaba] HediffComp_NarehateHealing has {overrideHealFactor: " +
                              $"{Props.overrideHealFactor}" +
                              "} but {isOverride: true}";
                    Log.WarningOnce(log,Gen.HashCombine(Pawn.thingIDNumber, "HediffComp_NarehateHealing.EffectiveHealFactor"));
                    return 1f;
                }
                return Props.overrideHealFactor;
            }
            var factor = ManosabaMod.Settings.narehateHealFactor;
            if (ApplyPostMultiplier)
            {
                factor *= ManosabaMod.Settings.postHealMultiplier * 10;
            }
            else if (ManosabaMod.Settings.isNarehateDownedDivisor && Pawn.Downed)
            {
                factor /= ManosabaMod.Settings.narehateDownedDivisor;
            }
            return factor;
        }
    }
    private float EffectiveBloodHealFactor 
    {
        get
        {
            if (Props.isOverride)
            {
                if (Props.overrideBloodFactor <= 0)
                {
                    var log = "[Manosaba] HediffComp_NarehateHealing has {overrideBloodFactor: " +
                              $"{Props.overrideBloodFactor}" +
                              "} but {isOverride: true}";
                    Log.WarningOnce(log,Gen.HashCombine(Pawn.thingIDNumber, "HediffComp_NarehateHealing.EffectiveBloodHealFactor"));
                    return 1f;
                }
                return Props.overrideBloodFactor;
            }
            var factor = ManosabaMod.Settings.narehateBloodHealFactor;
            if (ApplyPostMultiplier)
            {
                factor *= ManosabaMod.Settings.postHealMultiplier * 10;
            }
            else if (ManosabaMod.Settings.isNarehateDownedDivisor && Pawn.Downed)
            {
                factor /= ManosabaMod.Settings.narehateDownedDivisor;
            }
            return factor;
        }
    }
    private float RegenChance
    {
        get
        {
            var chance = 0.1f;
            if (Props.isOverride)
            {
                chance = Mathf.Max(0f,Mathf.Min(Props.overrideRegenFactor,1f));
                if (Props.overrideRegenFactor is <= 0 or > 1)
                {
                    var log = "[Manosaba] HediffComp_NarehateHealing has invalid {overrideRegenFactor: " +
                              $"{Props.overrideRegenFactor}" +
                              "} but {isOverride: true}. This will not cause any error but also not take any effect.";
                    Log.WarningOnce(log, Gen.HashCombine(Pawn.thingIDNumber, "HediffComp_NarehateHealing.RegenChance"));
                }
                return chance;
            }
            
            if (ApplyPostMultiplier)
            {
                chance /= 10;
            }
            else if (ManosabaMod.Settings.isNarehateDownedDivisor && Pawn.Downed)
            {
                chance /= ManosabaMod.Settings.narehateDownedDivisor;
            }
            return chance;
        }
    }
    private int TickInterval => (Props.isOverride || !ApplyPostMultiplier) ? 60 : 600;
    
    public override void CompPostTickInterval(ref float severityAdjustment, int delta)
    {
        base.CompPostTickInterval(ref severityAdjustment, delta);
        
        if (Pawn.IsHashIntervalTick(TickInterval, delta))
        {
            if (Props.applyStatusSwitch && !CachedHumanDummy.cachedIsFinished) return;
            if (ApplyPostMultiplier && !ManosabaMod.Settings.postAllowHeal) return;
            
            Utils.HealingUtils.TryHeal(Pawn, EffectiveHealFactor);
            
            if (ManosabaMod.Settings.isNarehateBloodHeal)
            {
                Utils.HealingUtils.TryBloodLoss(Pawn, EffectiveHealFactor);
            }

            if (Rand.Value <= RegenChance)
            {
                Utils.HealingUtils.TryRegenerate(Pawn, 1);
            }
        }
    }
    
    public override string CompDebugString()
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
                healPerDay /= 10f;
                bloodHealPerDay /= 10f;
            }
            var stringHeal = "HediffComp_NarehateHealing_TipHeal".Translate(Mathf.Round(healPerDay));
            var stringBlood = "HediffComp_NarehateHealing_TipBlood".Translate(Mathf.Round(bloodHealPerDay));
            result += stringHeal;
            if (ManosabaMod.Settings.isNarehateBloodHeal) result += "\n" + stringBlood;
            
            // 自愈部件说明
            var regenTickExpected = TickInterval / RegenChance;
            float regenCount;
            string tipRegenNumber;
            var regenTipUnit1 = "HediffComp_NarehateHealing_TipRegenUnit1".Translate();
            var regenTipUnit2 = "HediffComp_NarehateHealing_TipRegenUnit2".Translate();
            var regenTipUnit3 = "HediffComp_NarehateHealing_TipRegenUnit3".Translate();
            var regenTipUnit4 = "HediffComp_NarehateHealing_TipRegenUnit4".Translate();
            var regenTipUnit5 = "HediffComp_NarehateHealing_TipRegenUnit5".Translate();
            switch(regenTickExpected)
            {
                // 显示器官自愈：x日/处
                case >60000:
                    regenCount = Mathf.Round(regenTickExpected / 60000);
                    tipRegenNumber = regenCount.ToString("F0") + regenTipUnit1;
                    break;
                // 显示器官自愈：x处/日
                case >2500:
                    regenCount = Mathf.Round(60000 / regenTickExpected);
                    tipRegenNumber = regenCount.ToString("F0") + regenTipUnit2;
                    break;
                // 显示器官自愈：x处/小时
                case >600:
                    regenCount = Mathf.Round(2500 / regenTickExpected);
                    tipRegenNumber = regenCount.ToString("F0") + regenTipUnit3;
                    break;
                // 显示器官自愈：x秒/处
                case >60:
                    regenCount = Mathf.Round(regenTickExpected / 60);
                    tipRegenNumber = regenCount.ToString("F0") + regenTipUnit4;
                    break;
                // 显示器官自愈：x处/秒
                case >0:
                    regenCount = Mathf.Round(60 / regenTickExpected);
                    tipRegenNumber = regenCount.ToString("F0") + regenTipUnit5;
                    break;
                // 错误
                default:
                    Log.ErrorOnce("[Manosaba] HediffComp_NarehateHealing has {regenExpectTick: 0}",
                        Gen.HashCombine(Pawn.thingIDNumber, "HediffComp_NarehateHealing.CompDebugString"));
                    tipRegenNumber = "ERR";
                    break;
            }
            var stringRegen = "HediffComp_NarehateHealing_TipRegen".Translate() + tipRegenNumber;
            result += "\n" + stringRegen;
            return result;
        }
    }