using RimWorld;
using Verse;
using Verse.AI;

namespace UranvManosaba.Contents.MentalStates;

public class MentalState_MentalBreakFactor : MentalState_Berserk
{
    public override string InspectLine
    {
        get
        {
            var target = JobGiver_MentalBreakFactor.FindHatedTarget(pawn);
            var targetName = target != null ? target.LabelShort : "";
            return string.Format(def.baseInspectLine, pawn.LabelShort, targetName);
        }
    }

    public override void PostEnd()
    {
        base.PostEnd();
        if (!PawnUtility.ShouldSendNotificationAbout(pawn)) return;
        var target = JobGiver_MentalBreakFactor.FindHatedTarget(pawn);
        var targetName = target != null ? target.LabelShort : "";
        string msg = "MentalBreakFactor_Recovery".Translate(pawn.LabelShort, targetName);
        Messages.Message(msg, pawn, MessageTypeDefOf.SituationResolved, true);
    }
}