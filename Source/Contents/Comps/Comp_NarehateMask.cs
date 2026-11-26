using Verse;

namespace UranvManosaba.Contents.Comps;

public class CompProperties_NarehateMask : CompProperties
{
    public CompProperties_NarehateMask()
    {
        compClass = typeof(Comp_NarehateMask);
    }
}

public class Comp_NarehateMask : ThingComp
{
    internal static bool IsWornBy(Pawn pawn) =>
        pawn.apparel?.GetDirectlyHeldThings().Contains(ModDefOf.UmThingApparelNarehateMask) == true;

    public override void Notify_Equipped(Pawn pawn)
    {
        HediffComp_MutantDraft.Sync(pawn);
    }

    public override void Notify_Unequipped(Pawn pawn)
    {
        HediffComp_MutantDraft.Sync(pawn);
    }
}
