using UranvManosaba.Contents.Utils;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_HumanDummy : HediffCompProperties
{
    public HediffDef visibleHediffDef;
    public HediffDef narehateHediffDef;
    public HediffDef narehateHiddenHediffDef;
    public HediffDef hanmajyoHediffDef;
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
    public bool cachedIsFinished;
    public bool cachedIsNarehate;
    public bool cachedIsCured;
    public bool cachedIsDisplay;
    public bool shouldDisplay;
    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_Values.Look(ref cachedIsFinished, "cachedIsFinished", false);
        Scribe_Values.Look(ref cachedIsNarehate, "cachedIsNarehate", false);
        Scribe_Values.Look(ref cachedIsCured, "cachedIsCured", false);
        Scribe_Values.Look(ref cachedIsDisplay, "cachedIsDisplay", false);
        Scribe_Values.Look(ref shouldDisplay, "shouldDisplay", false);
    }

    ///====================================
    /// 外部修改 Pawn Dummy State
    //=====================================
    public static void SetDummyCured(Hediff parent)
    {
        var comp = parent.TryGetComp<HediffComp_HumanDummy>();
        if (comp != null)
        {
            comp.cachedIsCured = true;
            // 立刻刷新状态
            comp.MaintainFlags();
            comp.MaintainHediffs();
            comp.MaintainGene();
            comp.MaintainMentalState();
            if (ManosabaMod.Settings.debugMode)  Log.Message($"[Manosaba] SetCured:\ncachedIsCured={comp.cachedIsCured},\ncachedIsNarehate={comp.cachedIsNarehate},\ncachedIsDisplay={comp.cachedIsDisplay},\ncachedIsFinished={comp.cachedIsFinished},\nshouldDisplay={comp.shouldDisplay} (Comps.HediffComp_HumanDummy.SetDummyCured)");
            
        }
    }
    public static void SetDummyShouldDisplay(Pawn p)
    {
        var hediff = p?.health?.hediffSet?.GetFirstHediffOfDef(ModDefOf.UmHediffHumanDummy);
        var comp = hediff.TryGetComp<HediffComp_HumanDummy>();
        if (comp != null)
        {
            comp.shouldDisplay = true;
            // 立刻刷新状态
            comp.MaintainFlags();
            comp.MaintainHediffs();
            comp.MaintainGene();
            comp.MaintainMentalState();
            if (ManosabaMod.Settings.debugMode)  Log.Message($"[Manosaba] SetDisplay:\ncachedIsCured={comp.cachedIsCured},\ncachedIsNarehate={comp.cachedIsNarehate},\ncachedIsDisplay={comp.cachedIsDisplay},\ncachedIsFinished={comp.cachedIsFinished},\nshouldDisplay={comp.shouldDisplay} (Comps.HediffComp_HumanDummy.SetDummyShouldDisplay)");
                
        }
    }

    //====================================
    // 覆盖函数
    //=====================================
    // Tick 维护
    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        if (Pawn.IsHashIntervalTick(1000))
        {
            MaintainFlags();
            MaintainHediffs();
            MaintainGene();
            // 任意魔女因子暴露时解锁前置科技
            if (cachedIsDisplay) ResearchUtils.UnlockResearchPrereqs();
            // 发现在变种人上时
            if (Pawn.IsMutant) 
            {
                if (ManosabaMod.Settings.debugMode) Log.Message(
                    $"[Manosaba] Try to transfer [Majyoinshi] from Mutant {Pawn.Name.ToStringFull} to other colonists on the map (Comps.HediffComp_HumanDummy.CompPostTick)");
                // 尝试转移 Dummy 因子到地图上随机 Pawn
                RandomSelector.TryAddDummyToRandomPawnOnMap(Pawn.Map, inverseTemperature: ManosabaMod.Settings.inverseTemperature);
                // 然后自我移除 Dummy 因子
                Pawn.health.RemoveHediff(parent);
            }
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
        var visibleHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.visibleHediffDef);
        if (visibleHediff != null) Pawn.health.RemoveHediff(visibleHediff);
        var narehateHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.narehateHediffDef);
        if (narehateHediff != null) Pawn.health.RemoveHediff(narehateHediff);
        var narehateHiddenHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.narehateHiddenHediffDef);
        if (narehateHiddenHediff != null) Pawn.health.RemoveHediff(narehateHiddenHediff);
        var hanmajyoHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.hanmajyoHediffDef);
        if (hanmajyoHediff != null) Pawn.health.RemoveHediff(hanmajyoHediff);
        // 删除基因
        var gene = Pawn.genes?.GetGene(ModDefOf.UmGeneFactor);
        if (gene != null) Pawn.genes.RemoveGene(gene);
    }
    // 开发者显示 当前进度状态
    public override string CompLabelInBracketsExtra
    {
        get
        {
            if (cachedIsCured) return "H";
            if (cachedIsNarehate) return "N";
            if (cachedIsFinished) return "E";
            if (cachedIsDisplay) return "T";
            if (!cachedIsDisplay) return "F";
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
            
        cachedIsFinished = progressFinished;
        cachedIsNarehate = progressFinished && !cachedIsCured;
        cachedIsDisplay = progressDisplay || shouldDisplay;
            
        //Log.Warning($"[Manosaba] humandummy maintain flag:\ncachedIsCured={cachedIsCured},cachedIsNarehate={cachedIsNarehate},cachedIsDisplay={cachedIsDisplay},cachedIsFinished={cachedIsFinished},shouldDisplay={shouldDisplay};");
    }
    private void MaintainHediffs()
    {
        if (Pawn?.health?.hediffSet == null) return;
            
        var hasNarehate = Pawn.health.hediffSet.HasHediff(Props.narehateHediffDef);
        var hasNarehateHidden = Pawn.health.hediffSet.HasHediff(Props.narehateHiddenHediffDef);
        var hasHanmajyo = Pawn.health.hediffSet.HasHediff(Props.hanmajyoHediffDef);
        var hasVisible = Pawn.health.hediffSet.HasHediff(Props.visibleHediffDef);
            
        //Log.Warning($"[Manosaba] humandummy maintain hediff:\nhasNarehate={hasNarehate},hasNarehateHidden={hasNarehateHidden},hasHanmajyo={hasHanmajyo},hasVisible={hasVisible};\ncachedIsCured={cachedIsCured},cachedIsNarehate={cachedIsNarehate},cachedIsDisplay={cachedIsDisplay},cachedIsFinished={cachedIsFinished},shouldDisplay={shouldDisplay};");
            
        if (cachedIsCured)
        {
            if (!hasHanmajyo) Pawn.health.AddHediff(ModDefOf.UmHediffNarehateHealed);
        }
        if (cachedIsNarehate)
        {
            if (!hasNarehate) Pawn.health.AddHediff(ModDefOf.UmHediffNarehate);
            if (!hasNarehateHidden) Pawn.health.AddHediff(ModDefOf.UmHediffNarehateHidden);
        }
        if (cachedIsFinished)
        {
            parent.Severity = 1.0f;
            if (!hasVisible) return;
            var h = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.visibleHediffDef);
            Pawn.health.RemoveHediff(h);
        }
        else if (cachedIsDisplay)
        {
            if (!hasVisible)
            {
                var h = Pawn.health.AddHediff(Props.visibleHediffDef);
                h.Severity = parent.Severity;
            }
            else
            {
                var h = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.visibleHediffDef);
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
            case true when cachedIsDisplay:
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
                    cachedIsDisplay = true;
                }
                break;
            }
        }
    }

    // 精神
    private void MaintainMentalState()
    {
        //if (ManosabaMod.Settings.debugMode)  Log.Message($"[Manosaba] Current State:\ncachedIsCured={cachedIsCured},\ncachedIsNarehate={cachedIsNarehate},\ncachedIsDisplay={cachedIsDisplay},\ncachedIsFinished={cachedIsFinished},\nshouldDisplay={shouldDisplay}\n(Comps.HediffComp_HumanDummy.MaintainMentalState)");

        if (!cachedIsNarehate)
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