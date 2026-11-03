using UranvManosaba.Contents.Utils;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_YukiDummy : HediffCompProperties
{
    public HediffDef VisitorHediff;
    public HediffDef VisitorHiddenHediff;
    public HediffDef ColonistHediff;
    public HediffDef ColonistHiddenHediff;
    public HediffCompProperties_YukiDummy()
    {
        this.compClass = typeof(HediffComp_YukiDummy);
    }
}

public enum YukiForm
{
    Unknow,
    Vis,
    Col
}

public class HediffComp_YukiDummy : HediffComp
{
    private HediffCompProperties_YukiDummy Props => (HediffCompProperties_YukiDummy)props;
    private const int TickInterval = 1800;
    public YukiForm Form = YukiForm.Unknow;

    // 等待健康系统统一移除，让其他 Comp 的添加回调先执行完毕。
    private bool _shouldRemove;
    private bool _copyPending;
    public override bool CompShouldRemove => _shouldRemove || !YukiGeneralUtils.IsYuki(Pawn) || base.CompShouldRemove;

    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_Values.Look(ref Form, "form", YukiForm.Unknow);
    }

    public override void CopyFrom(HediffComp other)
    {
        base.CopyFrom(other);
        Form = ((HediffComp_YukiDummy)other).Form;
        _shouldRemove = false;
        _cachedComp = null;
        _copyPending = true;
    }

    // 维护循环
    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        if (!_copyPending && !Pawn.IsHashIntervalTick(TickInterval)) return;
        _copyPending = false;
        if (MaintainHediffs()) MaintainGene();
    }
    // 初始化
    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        base.CompPostPostAdd(dinfo);
        if (_copyPending) return;
        if (MaintainHediffs()) MaintainGene();
    }
    // 移除时清理关联 Hediff 和 Gene
    public override void CompPostPostRemoved()
    {
        base.CompPostPostRemoved();
        // 删除关联 Hediff
        var visitorHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.VisitorHediff);
        if (visitorHediff != null) Pawn.health.RemoveHediff(visitorHediff);
        var visitorHiddenHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.VisitorHiddenHediff);
        if (visitorHiddenHediff != null) Pawn.health.RemoveHediff(visitorHiddenHediff);
        var colonistHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.ColonistHediff);
        if (colonistHediff != null) Pawn.health.RemoveHediff(colonistHediff);
        var colonistHiddenHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.ColonistHiddenHediff);
        if (colonistHiddenHediff != null) Pawn.health.RemoveHediff(colonistHiddenHediff);
        // 错挂在其他种族上时，保留该宿主原有的魔女因子。
        if (!YukiGeneralUtils.IsYuki(Pawn)) return;
        var gene = Pawn.genes?.GetGene(ModDefOf.UmGeneFactor);
        if (gene != null) Pawn.genes.RemoveGene(gene);
    }
    // 输出状态
    public override string CompLabelInBracketsExtra
    {
        get
        {
            var state = Form == YukiForm.Col ? "COL" :
                Form == YukiForm.Vis ? "VIS" :
                "ERR";
            return state;
        }
    }
    // 返回是否完成维护；恢复旧类型或等待移除时，本轮不处理其他状态。
    private bool MaintainHediffs()
    {
        if (Pawn?.health?.hediffSet == null || CompShouldRemove) return false;
        // 维护 Yuki 的特殊 Hediff
        if (Form == YukiForm.Col)
        {
            var hasHediff = Pawn.health.hediffSet.HasHediff(Props.ColonistHediff);
            var hasHiddenHediff = Pawn.health.hediffSet.HasHediff(Props.ColonistHiddenHediff);
            if (!hasHediff) Pawn.health.AddHediff(Props.ColonistHediff);
            if (!hasHiddenHediff) Pawn.health.AddHediff(Props.ColonistHiddenHediff);
        }
        else if (Form == YukiForm.Vis)
        {
            var hasVisitorHediff = Pawn.health.hediffSet.HasHediff(Props.VisitorHediff);
            var hasVisitorHiddenHediff = Pawn.health.hediffSet.HasHediff(Props.VisitorHiddenHediff);
            if (!hasVisitorHediff) Pawn.health.AddHediff(Props.VisitorHediff);
            if (!hasVisitorHiddenHediff) Pawn.health.AddHediff(Props.VisitorHiddenHediff);
        }
        else
        {
            // 只在类型缺失时兼容旧存档；成功恢复后交由下次维护处理。
            var kind = Pawn.kindDef;
            Form = kind == ModDefOf.UmPawnKindYukiColonist ? YukiForm.Col :
                kind == ModDefOf.UmPawnKindYukiVisitor ? YukiForm.Vis : YukiForm.Unknow;
            _shouldRemove = Form == YukiForm.Unknow;
            return false;
        }
        // 防止魔女因子
        var humanDummyHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(ModDefOf.UmHediffHumanDummy);
        if (humanDummyHediff != null) Pawn.health.RemoveHediff(humanDummyHediff);
        var mutantDummyHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(ModDefOf.UmHediffMutantDummy);
        if (mutantDummyHediff != null) Pawn.health.RemoveHediff(mutantDummyHediff);
        return true;
    }
    // 维护基因: 小雪总是没有魔女因子的
    private void MaintainGene()
    {
        if (!ModsConfig.BiotechActive) return;
        var gene = Pawn?.genes?.GetGene(ModDefOf.UmGeneFactor);
        if (gene != null) Pawn.genes.RemoveGene(gene);
    }


    private Comp_YukiVisitor _cachedComp;
    private Comp_YukiVisitor CachedComp
    {
        get
        {
            _cachedComp ??= Pawn.TryGetComp<Comp_YukiVisitor>();
            return _cachedComp;
        }
    }
    public override string CompTipStringExtra
    {
        get
        {
            const string tipString = "Comp_YukiVisitor:";
            if (CachedComp is null) return tipString + " is null";
            if (Pawn.IsColonist) return tipString + " is colonist";
            var interactionSteps = "\n  - interactionSteps = " + CachedComp.InteractionSteps.ToString("F0");
            var isTale = "\n  - isTale = " + CachedComp.IsTale;
            var isCasted = "\n  - isCasted = " + CachedComp.IsCasted;
            var isWorking = "\n  - isWorking = " + CachedComp.IsWorking;
            var isInfLevel = "\n  - isInfLevel = " + CachedComp.IsInfLevel;
            return tipString + interactionSteps + isTale + isCasted + isWorking + isInfLevel;
        }
    }
}