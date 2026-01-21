using Verse;
using Verse.AI;

namespace UranvManosaba.Contents.MentalStates;

public class MentalBreakWorker_MentalBreakFactor : MentalBreakWorker
{
    public override bool TryStart(Pawn pawn, string reason, bool causedByMood)
    {
        var success = pawn.mindState.mentalStateHandler.TryStartMentalState(
            def.mentalState, 
            reason, 
            transitionSilently: false, 
            causedByMood: causedByMood
        );
        if (!success) return false;
        SendCustomLetter(pawn);
        return true;
    }

    private void SendCustomLetter(Pawn pawn)
    {
        var target = JobGiver_MentalBreakFactor.FindHatedTarget(pawn);
        var targetName = target != null ? target.LabelShort : "";
        var label = def.mentalState.beginLetterLabel ?? "Manosaba_LetterLabel_Break".Translate(); 
        var text = string.Format(def.mentalState.beginLetter, pawn.LabelShort, targetName);
        Find.LetterStack.ReceiveLetter(
            label, 
            text, 
            def.mentalState.beginLetterDef, 
            pawn
        );
    }
}