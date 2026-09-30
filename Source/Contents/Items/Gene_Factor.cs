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
        // 只有魔女残骸变种人才使用 MutantDummy，不能污染其他 mutant。
        if (pawn.mutant?.Def == ModDefOf.UmMutantNarehate)
        {
            if (!pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffMutantDummy))
            {
                pawn.health.AddHediff(ModDefOf.UmHediffMutantDummy);
            }
        }
        // 维护人类 dummy Hediff
        else if (!pawn.IsMutant && !pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffHumanDummy))
        {
            var h = pawn.health.AddHediff(ModDefOf.UmHediffHumanDummy);
            Comps.HediffComp_HumanDummy.SetDummyShouldDisplay(pawn);
        }
    }
}