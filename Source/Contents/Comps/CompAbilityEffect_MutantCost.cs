using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class CompProperties_MutantAbilityCost : CompProperties_AbilityEffect
{
    public float controlTimeCost = 0f;

    public bool allowedWhileDowned = false;

    public float SeverityCost => controlTimeCost/ManosabaMod.Settings.mutantFullCircle;

    public CompProperties_MutantAbilityCost()
    {
        compClass = typeof(CompAbilityEffect_MutantCost);
    }

    public override IEnumerable<string> ExtraStatSummary()
    {
        yield return string.Concat("Manosaba_MutantAbility_Cost".Translate(controlTimeCost.ToString("F1")));//, (100f*SeverityCost).ToString("F1") ));
    }
}

public class CompAbilityEffect_MutantCost : CompAbilityEffect
{
    public new CompProperties_MutantAbilityCost Props => (CompProperties_MutantAbilityCost)props;

    private bool MutantNarehateCanDoNow
    {
        get
        {
            var h = parent.pawn.health?.hediffSet?.GetFirstHediffOfDef(ModDefOf.UmHediffMutantCountdown);
            return h != null && !(h.Severity + 0.01f < Props.SeverityCost);
        }
    }

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        
        var p = parent.pawn;
        var h = p?.health?.hediffSet?.GetFirstHediffOfDef(ModDefOf.UmHediffMutantCountdown);
        if (h != null) h.Severity -= Props.SeverityCost;
    }

    public override bool GizmoDisabled(out string reason)
    {
        var h = parent.pawn.health?.hediffSet?.GetFirstHediffOfDef(ModDefOf.UmHediffMutantCountdown);
        if (h == null)
        {
            reason = "Manosaba_MutantAbility_NotMutant".Translate(parent.pawn);
            return true;
        }
        if (h.Severity + 0.01f < Props.SeverityCost)
        {
            reason = "Manosaba_MutantAbility_NotEnoughTime".Translate(parent.pawn);
            return true;
        }
        reason = null;
        return false;
    }

    public override bool AICanTargetNow(LocalTargetInfo target)
    {
        return MutantNarehateCanDoNow;
    }
}
