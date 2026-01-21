using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_YukiDummy : HediffCompProperties
{
    public HediffDef visitorHediff;
    public HediffDef visitorHiddenHediff;
    public HediffDef colonistHediff;
    public HediffDef colonistHiddenHediff;
    public HediffCompProperties_YukiDummy()
    {
        this.compClass = typeof(HediffComp_YukiDummy);
    }
}

public class HediffComp_YukiDummy : HediffComp
{
    private HediffCompProperties_YukiDummy Props => (HediffCompProperties_YukiDummy)props;
    private const int TickInterval = 1800;
    private bool IsColonist => Pawn.kindDef == ModDefOf.UmPawnKindYukiColonist;
    private bool IsVisitor => Pawn.kindDef == ModDefOf.UmPawnKindYukiVisitor;

    // 维护循环
    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        if (!Pawn.IsHashIntervalTick(TickInterval)) return;
        MaintainHediffs();
        MaintainGene();
    }
    // 初始化
    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        base.CompPostPostAdd(dinfo);
        MaintainHediffs();
        MaintainGene();
    }
    // 移除时清理关联 Hediff 和 Gene
    public override void CompPostPostRemoved()
    {
        base.CompPostPostRemoved();
        // 删除关联 Hediff
        var visitorHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.visitorHediff);
        if (visitorHediff != null) Pawn.health.RemoveHediff(visitorHediff);
        var visitorHiddenHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.visitorHiddenHediff);
        if (visitorHiddenHediff != null) Pawn.health.RemoveHediff(visitorHiddenHediff);
        var colonistHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.colonistHediff);
        if (colonistHediff != null) Pawn.health.RemoveHediff(colonistHediff);
        var colonistHiddenHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.colonistHiddenHediff);
        if (colonistHiddenHediff != null) Pawn.health.RemoveHediff(colonistHiddenHediff);
        // 删除基因
        var gene = Pawn.genes?.GetGene(ModDefOf.UmGeneFactor);
        if (gene != null) Pawn.genes.RemoveGene(gene);
    }
    // 输出状态
    public override string CompLabelInBracketsExtra
    {
        get
        {
            var state = IsColonist ? "COL" :
                IsVisitor ? "VIS" :
                "ERR";
            return state;
        }
    }
    // 维护 Hediffs
    private void MaintainHediffs()
    {
        if (Pawn?.health?.hediffSet == null) return;
        // 维护 Yuki 的特殊 Hediff
        if (IsColonist)
        {
            var hasHediff = Pawn.health.hediffSet.HasHediff(Props.colonistHediff);
            var hasHiddenHediff = Pawn.health.hediffSet.HasHediff(Props.colonistHiddenHediff);
            if (!hasHediff) Pawn.health.AddHediff(Props.colonistHediff);
            if (!hasHiddenHediff) Pawn.health.AddHediff(Props.colonistHiddenHediff);
        }
        else if (IsVisitor)
        {
            var hasVisitorHediff = Pawn.health.hediffSet.HasHediff(Props.visitorHediff);
            var hasVisitorHiddenHediff = Pawn.health.hediffSet.HasHediff(Props.visitorHiddenHediff);
            if (!hasVisitorHediff) Pawn.health.AddHediff(Props.visitorHediff);
            if (!hasVisitorHiddenHediff) Pawn.health.AddHediff(Props.visitorHiddenHediff);
        }
        else
        {
            Pawn.health.RemoveAllHediffs();
            return;
        }
        // 防止魔女因子
        var humanDummyHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(ModDefOf.UmHediffHumanDummy);
        if (humanDummyHediff != null) Pawn.health.RemoveHediff(humanDummyHediff);
        var mutantDummyHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(ModDefOf.UmHediffMutantDummy);
        if (mutantDummyHediff != null) Pawn.health.RemoveHediff(mutantDummyHediff);
    }
    // 维护基因: 小雪总是没有魔女因子的
    private void MaintainGene()
    {
        if (!ModsConfig.BiotechActive) return;
        var gene = Pawn?.genes?.GetGene(ModDefOf.UmGeneFactor);
        if (gene != null) Pawn.genes.RemoveGene(gene);
    }
}