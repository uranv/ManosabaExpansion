using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_MutantDraft : HediffCompProperties
{
    public HediffCompProperties_MutantDraft()
    {
        compClass = typeof(HediffComp_MutantDraft);
    }
}

public class HediffComp_MutantDraft : HediffComp
{
    public override bool CompShouldRemove => !ShouldApply(Pawn);

    private static bool ShouldApply(Pawn pawn) =>
        pawn.Drafted && pawn.mutant?.Def == ModDefOf.UmMutantNarehate;

    internal static void Sync(Pawn pawn)
    {
        if (pawn.health == null || pawn.Dead) return;

        var def = ModDefOf.UmHediffMutantDraft;
        if (ShouldApply(pawn))
        {
            var hediff = pawn.health.GetOrAddHediff(def);
            var severity = Comp_NarehateMask.IsWornBy(pawn) ? def.stages[1].minSeverity : def.initialSeverity;
            if (hediff.Severity != severity) hediff.Severity = severity;
        }
        else if (pawn.health.hediffSet.GetFirstHediffOfDef(def) is { } existingHediff)
        {
            pawn.health.RemoveHediff(existingHediff);
        }
    }
}
