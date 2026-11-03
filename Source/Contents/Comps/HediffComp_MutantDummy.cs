using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_MutantDummy : HediffCompProperties
{
    public HediffDef NarehateHediffDef;
    public HediffDef NarehateHiddenHediffDef;
    public HediffDef CountdownHediffDef;
    public HediffCompProperties_MutantDummy()
    {
        compClass = typeof(HediffComp_MutantDummy);
    }
}

public class HediffComp_MutantDummy : HediffComp
{
    private HediffCompProperties_MutantDummy Props => (HediffCompProperties_MutantDummy)props;

    // public override string CompTipStringExtra
    // {
    //     get
    //     {
    //         var body = " IsColonist: " + (Pawn.IsColonist ? "true" : "false")
    //             + "/n IsFreeColonist: " + (Pawn.IsFreeColonist ? "true" : "false")
    //             + "/n IsColonySubhumanPlayerControlled: " + (Pawn.IsColonySubhumanPlayerControlled ? "true" : "false");
    //         return body;
    //     }
    // }

    private const int TickInterval = 1000;

    // 宿主按当前身份校验；交由健康系统统一移除，避免后续 GiveAbility
    // 的添加回调在提前移除之后继续执行，留下孤立能力。
    private bool InvalidHost => Pawn.mutant?.Def != ModDefOf.UmMutantNarehate;
    public override bool CompShouldRemove => InvalidHost || base.CompShouldRemove;
    private bool _copyPending;

    private float _countdownSeverity = 0.0001f;
    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_Values.Look(ref _countdownSeverity, "temporalSeverity", 0.0001f);
    }

    public override void CopyFrom(HediffComp other)
    {
        base.CopyFrom(other);
        _countdownSeverity = ((HediffComp_MutantDummy)other)._countdownSeverity;
        _copyPending = true;
    }

    // 维护循环
    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        if (InvalidHost || (!_copyPending && !Pawn.IsHashIntervalTick(TickInterval))) return;
        _copyPending = false;
        MaintainHediffs();
        MaintainGene();
    }

    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        base.CompPostPostAdd(dinfo);
        if (_copyPending || InvalidHost) return;
        MaintainHediffs();
        MaintainGene();
    }

    public override void CompPostPostRemoved()
    {
        base.CompPostPostRemoved();
        var humanOwner = Pawn.IsMutant ? null : Pawn.health.hediffSet
            .GetFirstHediffOfDef(ModDefOf.UmHediffHumanDummy)?.TryGetComp<HediffComp_HumanDummy>();
        // 先移除倒计时，由它结束自己的暴走；缺失时才由管理组件补清理。
        var countdown = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.CountdownHediffDef);
        if (countdown != null) Pawn.health.RemoveHediff(countdown);
        else if (humanOwner?.RequiresNarehateMentalState != true &&
                 Pawn.MentalStateDef == ModDefOf.UmMentalBreakNarehate)
        {
            Pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
        }

        if (humanOwner == null)
        {
            var narehateHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.NarehateHediffDef);
            if (narehateHediff != null) Pawn.health.RemoveHediff(narehateHediff);
            if (!Pawn.health.hediffSet.HasHediff(Props.NarehateHediffDef))
            {
                var hidden = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.NarehateHiddenHediffDef);
                if (hidden != null) Pawn.health.RemoveHediff(hidden);
            }
            var gene = Pawn.genes?.GetGene(ModDefOf.UmGeneFactor);
            if (gene != null) Pawn.genes.RemoveGene(gene);
        }
    }

    // 维护 Hediffs
    private void MaintainHediffs()
    {
        // 防空检查
        if (Pawn?.health?.hediffSet == null) return;
        // 维护残骸 Hediff
        var hasNarehate = Pawn.health.hediffSet.HasHediff(Props.NarehateHediffDef);
        if (!hasNarehate) Pawn.health.AddHediff(Props.NarehateHediffDef);
        var hasNarehateHidden = Pawn.health.hediffSet.HasHediff(Props.NarehateHiddenHediffDef);
        if (!hasNarehateHidden) Pawn.health.AddHediff(Props.NarehateHiddenHediffDef);
        // 维护失控倒计时 Hediff
        var countdownHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.CountdownHediffDef);
        if (countdownHediff == null)
        {
            Pawn.health.AddHediff(Props.CountdownHediffDef).Severity = _countdownSeverity;
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