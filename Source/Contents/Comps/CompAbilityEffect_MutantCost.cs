using RimWorld;
using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class CompProperties_MutantAbilityCost : CompProperties_AbilityEffect
{
    public float BaseControlTimeCost = 0f;

    public bool AllowedWhileDowned = false;

    public float ControlTimeCost(Pawn pawn) => BaseControlTimeCost *
        (pawn.apparel?.GetDirectlyHeldThings().Contains(ModDefOf.UmThingApparelNarehateMantle) == true ? 0.5f : 1f);

    public CompProperties_MutantAbilityCost()
    {
        compClass = typeof(CompAbilityEffect_MutantCost);
    }
}

public class CompAbilityEffect_MutantCost : CompAbilityEffect
{
    public new CompProperties_MutantAbilityCost Props => (CompProperties_MutantAbilityCost)props;

    private float ControlTimeCost => Props.ControlTimeCost(parent.pawn);
    private float SeverityCost => ControlTimeCost / ManosabaMod.Settings.MutantFullCircle;

    public override string ExtraTooltipPart()
    {
        return "Manosaba_MutantAbility_Cost".Translate(ControlTimeCost.ToString("0.0#"));
    }

    private bool MutantNarehateCanDoNow
    {
        get
        {
            var h = parent.pawn.health?.hediffSet?.GetFirstHediffOfDef(ModDefOf.UmHediffMutantCountdown);
            return h != null && (h.Severity - SeverityCost > 0.0001f );
        }
    }

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);

        var p = parent.pawn;
        var h = p?.health?.hediffSet?.GetFirstHediffOfDef(ModDefOf.UmHediffMutantCountdown);
        if (h != null) h.Severity = Mathf.Max(0.0001f, h.Severity - SeverityCost);
    }

    public override bool GizmoDisabled(out string reason)
    {
        var h = parent.pawn.health?.hediffSet?.GetFirstHediffOfDef(ModDefOf.UmHediffMutantCountdown);
        if (h == null)
        {
            reason = "Manosaba_MutantAbility_NotMutant".Translate(parent.pawn);
            return true;
        }
        if (h.Severity - SeverityCost <= 0.0001f )
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
