using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace UranvManosaba.Contents.Comps;

public class Ability_MutantAbility : Ability
{
    public Ability_MutantAbility() : base() { }
    public Ability_MutantAbility(Pawn pawn) : base(pawn) { }
    public Ability_MutantAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override bool GizmoDisabled(out string reason)
    {
        if (CanCooldown && OnCooldown && (!def.cooldownPerCharge || RemainingCharges == 0))
        {
            reason = "AbilityOnCooldown".Translate(CooldownTicksRemaining.ToStringTicksToPeriod()).Resolve();
            return true;
        }
        if (UsesCharges && RemainingCharges <= 0)
        {
            reason = "AbilityNoCharges".Translate();
            return true;
        }
        if (!comps.NullOrEmpty())
        {
            foreach (var t in comps)
            {
                if (t.GizmoDisabled(out reason))
                {
                    return true;
                }
            }
        }
        var canCast = CanCast;
        if (!canCast.Accepted)
        {
            reason = canCast.Reason;
            return true;
        }
        var lord = pawn.GetLord();
        if (lord != null)
        {
            var acceptanceReport = lord.AbilityAllowed(this);
            if (!acceptanceReport)
            {
                reason = acceptanceReport.Reason;
                return true;
            }
        }
        if (!pawn.Drafted && def.disableGizmoWhileUndrafted && pawn.GetCaravan() == null && !DebugSettings.ShowDevGizmos)
        {
            reason = "AbilityDisabledUndrafted".Translate();
            return true;
        }
        if (pawn.DevelopmentalStage.Baby())
        {
            reason = "IsIncapped".Translate(pawn.LabelShort, pawn);
            return true;
        }
        // =========================================================
        // 检查倒地状态
        // =========================================================
        if (pawn.Downed)
        {
            var allowedWhileDowned = false;
            var costComp = CompOfType<CompAbilityEffect_MutantCost>();
            if (costComp != null && costComp.Props.allowedWhileDowned)
            {
                allowedWhileDowned = true;
            }
            if (!allowedWhileDowned)
            {
                reason = "CommandDisabledUnconscious".TranslateWithBackup("CommandCallRoyalAidUnconscious").Formatted(pawn);
                return true;
            }
        }
        // =========================================================
        if (pawn.Deathresting)
        {
            reason = "CommandDisabledDeathresting".Translate(pawn);
            return true;
        }
        if (def.casterMustBeCapableOfViolence && pawn.WorkTagIsDisabled(WorkTags.Violent))
        {
            reason = "IsIncapableOfViolence".Translate(pawn.LabelShort, pawn);
            return true;
        }
        if (!CanQueueCast)
        {
            reason = "AbilityAlreadyQueued".Translate();
            return true;
        }
        reason = null;
        return false;
    }
}