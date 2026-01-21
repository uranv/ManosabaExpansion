using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Items;

// 摄入tredecim：添加药剂Hediff + 检查Dummy处死
public class IngestionOutcomeDoer_Tredecim : IngestionOutcomeDoer
{
    protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
    {
        if (pawn == null) return;
        
        var potionDef = ModDefOf.UmHediffTredecim;
        if (potionDef != null)
        {
            pawn.health.AddHediff(potionDef);
        }
        
        var dummyDef = ModDefOf.UmHediffHumanDummy;
        var mutantDummyDef = ModDefOf.UmHediffMutantDummy;

        var hasDummy = dummyDef != null && pawn.health?.hediffSet?.HasHediff(dummyDef) != false;
        var hasMutantDummy = mutantDummyDef != null && pawn.health?.hediffSet?.HasHediff(mutantDummyDef) != false;

        if (!hasDummy && !hasMutantDummy) return;
        // 发送消息提示
        var text = "Manosaba_Message_KilledByTredecim".Translate(pawn.LabelShort);
        Messages.Message(text, pawn, MessageTypeDefOf.NegativeEvent);
        pawn.Kill(null, null);
    }
}