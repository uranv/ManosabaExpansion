using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_NarehateHealingReveal : HediffCompProperties
{
    public bool applyPostFactor = false;

    public HediffCompProperties_NarehateHealingReveal()
    {
        compClass = typeof(HediffComp_NarehateHealingReveal);
    }
}

public class HediffComp_NarehateHealingReveal : HediffComp
{
    private HediffCompProperties_NarehateHealingReveal Props => (HediffCompProperties_NarehateHealingReveal)props;
        
    public override string CompTipStringExtra
    {
        get
        {
            float varHeal = ManosabaMod.Settings.factorHealing;
            float varBlood = ManosabaMod.Settings.factorBloodHealing;
            if (Props.applyPostFactor)
            {
                varHeal *= ManosabaMod.Settings.postFactorHealing;
                varBlood *= ManosabaMod.Settings.postFactorHealing;
            }
            string stringHeal = "HediffComp_NarehateHealingReveal_TipHeal".Translate(Mathf.Round(varHeal));
            string stringBlood = "HediffComp_NarehateHealingReveal_TipBlood".Translate(Mathf.Round(varBlood));
            string result = string.Empty;
                
            if (!Props.applyPostFactor) result = stringHeal + "\n" + stringBlood;
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