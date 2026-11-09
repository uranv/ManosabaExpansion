using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Items;

// 摄入tredecim：添加药剂Hediff + 检查Dummy处死
public class IngestionOutcomeDoer_Tredecim : IngestionOutcomeDoer
{
    protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
    {
        if (pawn?.health == null) return;
        pawn.health.AddHediff(ModDefOf.UmHediffTredecim);
        Utils.TredecimUtils.KillIfFactorAndTredecim(pawn);
    }
}
