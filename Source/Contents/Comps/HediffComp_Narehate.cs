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

    // 实现治愈: 当被移除时,自动添加 [半魔女] Hediff
    public override void CompPostPostRemoved()
    {
        base.CompPostPostRemoved();
            
        // 修改 dummy cured state
        var dummyHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(ModDefOf.UmHediffHumanDummy);
        HediffComp_HumanDummy.SetDummyCured(dummyHediff);
            
        // 强制刷新渲染器
        Utils.NarehateUtils.RefreshPawnGraphics(Pawn);

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