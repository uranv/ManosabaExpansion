using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_NarehateHealing : HediffCompProperties
{
    public bool checkStatus = false;
    public bool applyPostFactor = false;
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
    private bool ApplyPostFactor
    {
        get
        {
            if (!Props.checkStatus) return Props.applyPostFactor;
            return !CachedHumanDummy.cachedIsNarehate;
        }
    }

    private bool IsOverrideHealFactor => Props.overrideHealFactor != -1;
    private bool IsOverrideBloodFactor => Props.overrideBloodFactor != -1;
    private bool IsOverrideRegenFactor => Props.overrideRegenTick != -1;
        
        
    private float EffectiveHealingFactor => IsOverrideHealFactor ? 
        Props.overrideHealFactor :
        ManosabaMod.Settings.factorHealing * (ApplyPostFactor ? ManosabaMod.Settings.postFactorHealing * 10f : 1);

    private float EffectiveBloodHealingFactor => IsOverrideBloodFactor ?
        Props.overrideBloodFactor :
        ManosabaMod.Settings.factorBloodHealing * (ApplyPostFactor ? ManosabaMod.Settings.postFactorHealing * 10f : 1);

    private int HealingTickInterval => ApplyPostFactor ? 600 : 60;
    private int RegenTickInterval => IsOverrideRegenFactor ?
        Props.overrideRegenTick :
        ApplyPostFactor ? 60000 :
            900;
        
        
        
        
    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        //if(parent.pawn.IsMutant) Log.ErrorOnce($"{parent.def.defName} => heal:[{IsOverrideHealFactor}, {Props.overrideHealFactor}], regTick:{RegenTickInterval}",11);
        if (Props.checkStatus && !CachedHumanDummy.cachedIsFinished) return;
            
        if (Pawn.IsHashIntervalTick(HealingTickInterval))
        {
            if (!ApplyPostFactor || ManosabaMod.Settings.isPostHealing)
            {
                Utils.HealingUtils.TryHeal(Pawn, EffectiveHealingFactor);
            }
            if (!ApplyPostFactor || ManosabaMod.Settings.isBloodHealing)
            {
                Utils.HealingUtils.TryBloodLoss(Pawn, EffectiveBloodHealingFactor);
            }
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
            if (Props.checkStatus && !CachedHumanDummy.cachedIsFinished) return result;
                
            var varHeal = ManosabaMod.Settings.factorHealing;
            var varBlood = ManosabaMod.Settings.factorBloodHealing;
            if (ApplyPostFactor)
            {
                varHeal *= ManosabaMod.Settings.postFactorHealing;
                varBlood *= ManosabaMod.Settings.postFactorHealing;
            }
                
            if (IsOverrideHealFactor) varHeal = Props.overrideHealFactor;
            if (IsOverrideBloodFactor) varBlood = Props.overrideBloodFactor;
                
            var stringHeal = "HediffComp_NarehateHealing_TipHeal".Translate(Mathf.Round(varHeal));
            var stringBlood = "HediffComp_NarehateHealing_TipBlood".Translate(Mathf.Round(varBlood));
 
                
            if (!ApplyPostFactor) result = stringHeal + "\n" + stringBlood;
            else
            {
                bool var1 = ManosabaMod.Settings.isPostHealing;
                bool var2 = ManosabaMod.Settings.isBloodHealing;
                if (var1) result += stringHeal;
                if (var1 && var2) result += "\n";
                if (var2) result += stringBlood;
            }
            return result;
        }
    }
}