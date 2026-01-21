using Verse;

namespace UranvManosaba.Contents.Items;

public class Gene_Factor : Gene
{
    // 基因被添加时初始化
    public override void PostAdd()
    {
        base.PostAdd();
        EnsureDummyExists();
    }
        
    // 低频率检查 manager Hediff
    // public override void Tick()
    // {
    //     base.Tick();
    //     if (pawn.IsHashIntervalTick(60000))
    //     {
    //         EnsureDummyExists();
    //     }
    // }

    private void EnsureDummyExists()
    {
        if (pawn?.Map == null || pawn.Dead || pawn?.health?.hediffSet == null) return;
        // 维护变种人 dummy Hediff
        if (pawn.IsMutant)
        {
            if (!pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffMutantDummy))
            {
                pawn.health.AddHediff(ModDefOf.UmHediffMutantDummy);
            }
        }
        // 维护人类 dummy Hediff
        else if (!pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffHumanDummy))
        {
            var h = pawn.health.AddHediff(ModDefOf.UmHediffHumanDummy);
            Comps.HediffComp_HumanDummy.SetDummyShouldDisplay(pawn);
        }
    }
}