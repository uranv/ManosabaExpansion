using UranvManosaba.Contents.Utils;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_HumanDummy : HediffCompProperties
{
    public HediffDef VisibleHediffDef;
    public HediffDef NarehateHediffDef;
    public HediffDef NarehateHiddenHediffDef;
    public HediffDef HanmajyoHediffDef;
    public HediffCompProperties_HumanDummy()
    {
        this.compClass = typeof(HediffComp_HumanDummy);
    }
}

public class HediffComp_HumanDummy : HediffComp
{
    private HediffCompProperties_HumanDummy Props => (HediffCompProperties_HumanDummy)props;
    private HediffComp_Progress _cachedProgress;
    private HediffComp_Progress CachedProgress
    {
        get
        {
            _cachedProgress ??= this.parent.TryGetComp<HediffComp_Progress>();
            return _cachedProgress;
        }
    }
    // 保存状态
    public bool CachedIsFinished;
    public bool CachedIsNarehate;
    public bool CachedIsCured;
    public bool CachedIsDisplay;
    public bool ShouldDisplay;
    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_Values.Look(ref CachedIsFinished, "cachedIsFinished", false);
        Scribe_Values.Look(ref CachedIsNarehate, "cachedIsNarehate", false);
        Scribe_Values.Look(ref CachedIsCured, "cachedIsCured", false);
        Scribe_Values.Look(ref CachedIsDisplay, "cachedIsDisplay", false);
        Scribe_Values.Look(ref ShouldDisplay, "shouldDisplay", false);
    }

    ///====================================
    /// 外部修改 Pawn Dummy State
    //=====================================
    public static void SetDummyCured(Hediff parent)
    {
        var comp = parent.TryGetComp<HediffComp_HumanDummy>();
        // 不维护变种人状态
        if (comp == null || comp.Pawn.IsMutant) return;
        comp.CachedIsCured = true;
        // 立刻刷新状态
        comp.MaintainFlags();
        comp.MaintainHediffs();
        comp.MaintainGene();
        comp.MaintainMentalState();
        if (ManosabaMod.Settings.debugMode)  Log.Message($"[Manosaba] SetCured:\ncachedIsCured={comp.CachedIsCured},\ncachedIsNarehate={comp.CachedIsNarehate},\ncachedIsDisplay={comp.CachedIsDisplay},\ncachedIsFinished={comp.CachedIsFinished},\nshouldDisplay={comp.ShouldDisplay} (Comps.HediffComp_HumanDummy.SetDummyCured)");
    }
    public static void SetDummyShouldDisplay(Pawn p)
    {
        var hediff = p?.health?.hediffSet?.GetFirstHediffOfDef(ModDefOf.UmHediffHumanDummy);
        var comp = hediff.TryGetComp<HediffComp_HumanDummy>();
        // 不维护变种人状态
        if (comp == null || comp.Pawn.IsMutant) return;
        comp.ShouldDisplay = true;
        // 立刻刷新状态
        comp.MaintainFlags();
        comp.MaintainHediffs();
        comp.MaintainGene();
        comp.MaintainMentalState();
        if (ManosabaMod.Settings.debugMode)  Log.Message($"[Manosaba] SetDisplay:\ncachedIsCured={comp.CachedIsCured},\ncachedIsNarehate={comp.CachedIsNarehate},\ncachedIsDisplay={comp.CachedIsDisplay},\ncachedIsFinished={comp.CachedIsFinished},\nshouldDisplay={comp.ShouldDisplay} (Comps.HediffComp_HumanDummy.SetDummyShouldDisplay)");
    }
    public static bool QueryIsDisplayed(Pawn p)
    {
        var h = p?.health?.hediffSet?.GetFirstHediffOfDef(ModDefOf.UmHediffHumanDummy);
        var c = h?.TryGetComp<HediffComp_HumanDummy>();
        return c is { ShouldDisplay: true };
    }

    //====================================
    // 覆盖函数
    //=====================================
    // Tick 维护
    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        // HumanDummy 不属于任何 mutant；直接转移并清理自身
        if (Pawn.IsMutant)
        {
            if (!Pawn.IsHashIntervalTick(1000)) return;
            if (ManosabaMod.Settings.debugMode) Log.Message(
                $"[Manosaba] Try to transfer [Majyoinshi] from Mutant {Pawn.Name.ToStringFull} to other colonists on the map (Comps.HediffComp_HumanDummy.CompPostTick)");
            RandomSelector.TryAddDummyToRandomPawnOnMap(Pawn.Map, inverseTemperature: ManosabaMod.Settings.inverseTemperature);
            Pawn.health.RemoveHediff(parent);
            return;
        }
        // 维护类人 Pawn 状态
        if (Pawn.IsHashIntervalTick(1000))
        {
            MaintainFlags();
            MaintainHediffs();
            MaintainGene();
            // 任意魔女因子暴露时解锁前置科技
            if (CachedIsDisplay) ResearchUtils.UnlockResearchPrereqs();
        }
        // 较短间隔维护精神状态
        if (Pawn.IsHashIntervalTick(300))
        {
            MaintainMentalState();
        }
    }
    // 初始化
    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        base.CompPostPostAdd(dinfo);
        // 加入时检测宿主，不是变种人
        if (Pawn.IsMutant) return;
        MaintainFlags();
        MaintainHediffs();
        MaintainGene();
        MaintainMentalState();
    }
    // 同时删除所有关联特性
    public override void CompPostPostRemoved()
    {
        base.CompPostPostRemoved();
        // 删除进度 Hediff
        var visibleHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.VisibleHediffDef);
        if (visibleHediff != null) Pawn.health.RemoveHediff(visibleHediff);
        var narehateHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.NarehateHediffDef);
        if (narehateHediff != null) Pawn.health.RemoveHediff(narehateHediff);
        var narehateHiddenHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.NarehateHiddenHediffDef);
        if (narehateHiddenHediff != null) Pawn.health.RemoveHediff(narehateHiddenHediff);
        var hanmajyoHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.HanmajyoHediffDef);
        if (hanmajyoHediff != null) Pawn.health.RemoveHediff(hanmajyoHediff);
        // 删除基因
        var gene = Pawn.genes?.GetGene(ModDefOf.UmGeneFactor);
        if (gene != null) Pawn.genes.RemoveGene(gene);
        // 停止精神状态
        if (Pawn.MentalStateDef == ModDefOf.UmMentalBreakNarehate)
        {
            Pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
        }
    }
    // 开发者显示 当前进度状态
    public override string CompLabelInBracketsExtra
    {
        get
        {
            if (CachedIsCured) return "H";
            if (CachedIsNarehate) return "N";
            if (CachedIsFinished) return "E";
            if (CachedIsDisplay) return "T";
            if (!CachedIsDisplay) return "F";
            return base.CompLabelInBracketsExtra;
        }
    }

    //====================================
    // 维护进度、基因、关联 Hediff
    //=====================================
    private void MaintainFlags()
    {
        var progressFinished = CachedProgress?.isFinished ?? false;
        var progressDisplay = CachedProgress?.isDisplay ?? false;
        CachedIsFinished = progressFinished;
        CachedIsNarehate = progressFinished && !CachedIsCured;
        CachedIsDisplay = progressDisplay || ShouldDisplay;
        //Log.Warning($"[Manosaba] humandummy maintain flag:\ncachedIsCured={cachedIsCured},cachedIsNarehate={cachedIsNarehate},cachedIsDisplay={cachedIsDisplay},cachedIsFinished={cachedIsFinished},shouldDisplay={shouldDisplay};");
    }
    private void MaintainHediffs()
    {
        if (Pawn?.health?.hediffSet == null) return;
        var hasNarehate = Pawn.health.hediffSet.HasHediff(Props.NarehateHediffDef);
        var hasNarehateHidden = Pawn.health.hediffSet.HasHediff(Props.NarehateHiddenHediffDef);
        var hasHanmajyo = Pawn.health.hediffSet.HasHediff(Props.HanmajyoHediffDef);
        var hasVisible = Pawn.health.hediffSet.HasHediff(Props.VisibleHediffDef);
        //Log.Warning($"[Manosaba] humandummy maintain hediff:\n hasNarehate={hasNarehate},hasNarehateHidden={hasNarehateHidden},hasHanmajyo={hasHanmajyo},hasVisible={hasVisible};\ncachedIsCured={cachedIsCured},cachedIsNarehate={cachedIsNarehate},cachedIsDisplay={cachedIsDisplay},cachedIsFinished={cachedIsFinished},shouldDisplay={shouldDisplay};");
        if (CachedIsCured)
        {
            if (!hasHanmajyo) Pawn.health.AddHediff(ModDefOf.UmHediffNarehateHealed);
        }
        if (CachedIsNarehate)
        {
            if (!hasNarehate) Pawn.health.AddHediff(ModDefOf.UmHediffNarehate);
            if (!hasNarehateHidden) Pawn.health.AddHediff(ModDefOf.UmHediffNarehateHidden);
        }
        if (CachedIsFinished)
        {
            parent.Severity = 1.0f;
            if (!hasVisible) return;
            var h = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.VisibleHediffDef);
            Pawn.health.RemoveHediff(h);
        }
        else if (CachedIsDisplay)
        {
            if (!hasVisible)
            {
                var h = Pawn.health.AddHediff(Props.VisibleHediffDef);
                h.Severity = parent.Severity;
            }
            else
            {
                var h = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.VisibleHediffDef);
                h.Severity = parent.Severity;
            }
        }


    }

    // 基因
    private void MaintainGene()
    {
        if (!ModsConfig.BiotechActive) return;
        if (Pawn == null) return;
        var gene = Pawn.genes.GetGene(ModDefOf.UmGeneFactor);
        switch (ModDefOf.UmResearchProjectGene.IsFinished)
        {
            // 完成对应科技前, 基因总是隐藏 (无论是否显示)
            case false:
            {
                if (gene != null)
                {
                    Pawn.genes.RemoveGene(gene);
                }
                break;
            }
            // 科技解锁后, 可见时总存在基因
            case true when CachedIsDisplay:
            {
                if (gene == null)
                {
                    Pawn.genes.AddGene(ModDefOf.UmGeneFactor, true);
                }
                break;
            }
            case true:
            {
                if (gene != null)
                {
                    CachedIsDisplay = true;
                }
                break;
            }
        }
    }

    // 精神
    private void MaintainMentalState()
    {
        //if (ManosabaMod.Settings.debugMode)  Log.Message($"[Manosaba] Current State:\n cachedIsCured={cachedIsCured},\ncachedIsNarehate={cachedIsNarehate},\ncachedIsDisplay={cachedIsDisplay},\ncachedIsFinished={cachedIsFinished},\nshouldDisplay={shouldDisplay}\n(Comps.HediffComp_HumanDummy.MaintainMentalState)");

        if (!CachedIsNarehate)
        {
            if (Pawn.MentalStateDef == ModDefOf.UmMentalBreakNarehate)
            {
                Pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
            }
            return;
        }
        if (Pawn.Dead || Pawn.Downed) return;
        if (Pawn.MentalStateDef == ModDefOf.UmMentalBreakNarehate) return;
        // 进入魔女残骸状态
        Pawn.mindState.mentalStateHandler.TryStartMentalState(
            ModDefOf.UmMentalBreakNarehate,
            null,
            true,
            true,
            false,
            null,
            true
        );
    }
}