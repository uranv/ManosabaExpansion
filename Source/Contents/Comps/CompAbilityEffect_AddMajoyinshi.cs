using RimWorld;
using Verse;
using Verse.Sound;

namespace UranvManosaba.Contents.Comps;

public class CompProperties_AddMajoyinshi : CompProperties_AbilityEffect
{
    public CompProperties_AddMajoyinshi()
    {
        compClass = typeof(CompAbilityEffect_AddMajoyinshi);
    }
}

public class CompAbilityEffect_AddMajoyinshi : CompAbilityEffect
{
    public new CompProperties_AddMajoyinshi Props => (CompProperties_AddMajoyinshi)props;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        var p = target.Pawn;
        var map = p.MapHeld;
        var pos = p.PositionHeld;
        if (p.health?.hediffSet == null || map == null) return;
        base.Apply(target, dest);

        var h = ModDefOf.UmHediffHumanDummy;
        if (!p.health.hediffSet.HasHediff(h))
        {
            p.health.AddHediff(h);
            HediffComp_HumanDummy.SetDummyShouldDisplay(p);
        }
            
        var s = SoundDef.Named("PsychicSoothePulserCast");
        s?.PlayOneShot(new TargetInfo(pos, map));

    }

    public override bool AICanTargetNow(LocalTargetInfo target) => false;

    public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest) => Valid(target);

    public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
    {
        var pawn = target.Pawn;
        if (pawn?.health?.hediffSet == null)
        {
            return false;
        }

        if (!pawn.RaceProps.Humanlike || pawn.IsMutant)
        {
            if (throwMessages)
            {
                Messages.Message("Manosaba_AbilityAdd_NotHumanlike".Translate(pawn.Named("PAWN")), pawn,
                    MessageTypeDefOf.RejectInput);
            }

            return false;
        }

        if (pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffHumanDummy))
        {
            if (throwMessages)
            {
                Messages.Message("Manosaba_AbilityAdd_AlreadyHas".Translate(pawn.Named("PAWN")), pawn,
                    MessageTypeDefOf.RejectInput);
            }

            return false;
        }

        if (pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffYukiDummy))
        {
            if (throwMessages)
            {
                Messages.Message("Manosaba_AbilityAdd_CannotBeYuki".Translate(pawn.Named("PAWN")), pawn,
                    MessageTypeDefOf.RejectInput);
            }

            return false;
        }

        return true;
    }
}