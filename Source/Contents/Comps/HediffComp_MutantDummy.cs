using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_MutantDummy : HediffCompProperties
{
    public HediffDef narehateHediffDef;
    public HediffDef narehateHiddenHediffDef;
    public HediffDef countdownHediffDef;
    public HediffCompProperties_MutantDummy()
    {
        compClass = typeof(HediffComp_MutantDummy);
    }
}

public class HediffComp_MutantDummy : HediffComp
{
    private HediffCompProperties_MutantDummy Props => (HediffCompProperties_MutantDummy)props;
        
    private const int TickInterval = 1000;
        
    private float _countdownSeverity = 0.0001f;
    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_Values.Look(ref _countdownSeverity, "temporalSeverity", 0.0001f);
    }

    // 维护循环
    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        if (!Pawn.IsHashIntervalTick(TickInterval)) return;
        MaintainHediffs();
        MaintainGene();
    }

    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        base.CompPostPostAdd(dinfo);
        MaintainHediffs();
        MaintainGene();
    }

    public override void CompPostPostRemoved()
    {
        base.CompPostPostRemoved();
        // 删除关联 Hediff
        var narehateHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.narehateHediffDef);
        if (narehateHediff != null) Pawn.health.RemoveHediff(narehateHediff);
        var narehateHiddenHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.narehateHiddenHediffDef);
        if (narehateHiddenHediff != null) Pawn.health.RemoveHediff(narehateHiddenHediff);
        var temporalHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.countdownHediffDef);
        if (temporalHediff != null) Pawn.health.RemoveHediff(temporalHediff);
        // 删除基因
        var gene = Pawn.genes?.GetGene(ModDefOf.UmGeneFactor);
        if (gene != null) Pawn.genes.RemoveGene(gene);
    }

    // 维护 Hediffs
    private void MaintainHediffs()
    {
        // 防空检查
        if (Pawn?.health?.hediffSet == null) return;
        // 维护残骸 Hediff
        var hasNarehate = Pawn.health.hediffSet.HasHediff(Props.narehateHediffDef);
        if (!hasNarehate) Pawn.health.AddHediff(Props.narehateHediffDef);
        var hasNarehateHidden = Pawn.health.hediffSet.HasHediff(Props.narehateHiddenHediffDef);
        if (!hasNarehateHidden) Pawn.health.AddHediff(Props.narehateHiddenHediffDef);
        // 维护失控倒计时 Hediff
        var countdownHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.countdownHediffDef);
        if (countdownHediff == null)
        {
            Pawn.health.AddHediff(Props.countdownHediffDef).Severity = _countdownSeverity;
        }
        else
        {
            _countdownSeverity = countdownHediff.Severity;
        }
        // 防止错误治愈
        var hasHanmajyo = Pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffNarehateHealed);
        if (!hasHanmajyo) return;
        var hanmajyoHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(ModDefOf.UmHediffNarehateHealed);
        Pawn.health.RemoveHediff(hanmajyoHediff);
    }
    // 维护基因
    private void MaintainGene()
    {
        if (!ModsConfig.BiotechActive) return;
        if (Pawn?.genes == null) return;
        var gene = Pawn.genes.GetGene(ModDefOf.UmGeneFactor);
        switch (ModDefOf.UmResearchProjectGene.IsFinished)
        {
            // 完成对应科技前, 基因总是隐藏
            case false:
            {
                if (gene != null)
                {
                    Pawn.genes.RemoveGene(gene);
                }
                break;
            }
            // 科技解锁后, 尝试维护基因状态
            case true:
            {
                if (gene == null)
                {
                    Pawn.genes.AddGene(ModDefOf.UmGeneFactor, true);
                }
                break;
            }
        }
    }
}