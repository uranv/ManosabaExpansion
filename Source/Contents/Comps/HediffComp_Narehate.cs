using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_Narehate : HediffCompProperties
{
    public HediffDef hiddenHediffDef;
    public HediffDef curedHediffDef;
    public HediffCompProperties_Narehate()
    {
        this.compClass = typeof(HediffComp_Narehate);
    }
}


public class HediffComp_Narehate : HediffComp
{
    private HediffCompProperties_Narehate Props => (HediffCompProperties_Narehate)props;

    // 初始化
    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        base.CompPostPostAdd(dinfo);
        // 加入时同时添加 narehateHidden
        var narehateHiddenHediff = Pawn.health.hediffSet.HasHediff(Props.hiddenHediffDef);
        if (!narehateHiddenHediff)
        {
            Pawn.health.AddHediff(Props.hiddenHediffDef);
        }
    }

    // 只有仍持有 HumanDummy 的非 mutant 才将移除残骸视为治愈。
    public override void CompPostPostRemoved()
    {
        base.CompPostPostRemoved();

        // 若 Pawn 健康组件已销毁？则跳过处理
        if (Pawn?.health == null) return;

        // 清理错误 dummy 时也要刷新外观，但不能触发治愈副作用。
        Utils.NarehateUtils.RefreshPawnGraphics(Pawn);

        var dummyHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(ModDefOf.UmHediffHumanDummy);
        if (Pawn.IsMutant || dummyHediff == null) return;

        // 修改 HumanDummy cured state；UmMutantNarehate 仍由 MutantDummy 维护残骸。
        HediffComp_HumanDummy.SetDummyCured(dummyHediff);

        // 播放特效
        Utils.NarehateUtils.EffecterNarehateTrans(Pawn);

        // 添加替换 Hediff
        var narehateHiddenHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.hiddenHediffDef);
        if (narehateHiddenHediff != null)
        {
            Pawn.health.RemoveHediff(narehateHiddenHediff);
        }
        var hanmajyoHediff = Pawn.health.hediffSet.HasHediff(Props.curedHediffDef);
        if (Pawn.Dead) return;
        if (!hanmajyoHediff)
        {
            Pawn.health.AddHediff(Props.curedHediffDef);
        }
            
        // 强制结束敌对精神状态 (让小人恢复可控)
        if (Pawn.InMentalState)
        {
            Pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
        }
    }
}