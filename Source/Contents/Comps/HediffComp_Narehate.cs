using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_Narehate : HediffCompProperties
{
    public HediffDef HiddenHediffDef;
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
        if (!Pawn.health.hediffSet.HasHediff(Props.HiddenHediffDef))
        {
            Pawn.health.AddHediff(Props.HiddenHediffDef);
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

        // 有效 mutant 管理组件仍需要这些共享状态，移除显示项不触发治愈。
        if ((Pawn.mutant?.Def == ModDefOf.UmMutantNarehate &&
             Pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffMutantDummy)) ||
            Pawn.health.hediffSet.HasHediff(parent.def)) return;

        var hidden = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.HiddenHediffDef);
        if (hidden != null) Pawn.health.RemoveHediff(hidden);

        var dummyHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(ModDefOf.UmHediffHumanDummy);
        var humanOwner = dummyHediff?.TryGetComp<HediffComp_HumanDummy>();
        if (Pawn.IsMutant || humanOwner == null || Pawn.Dead)
        {
            if (Pawn.MentalStateDef == ModDefOf.UmMentalBreakNarehate &&
                Pawn.health.hediffSet.GetFirstHediffOfDef(ModDefOf.UmHediffMutantCountdown)
                    ?.TryGetComp<HediffComp_Countdown_Mutant>()?.RequiresNarehateMentalState != true)
            {
                Pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
            }
            return;
        }

        // 治愈状态、治愈 Hediff 和相关精神状态统一由 HumanDummy 维护。
        var wasCured = humanOwner.CachedIsCured;
        HediffComp_HumanDummy.SetDummyCured(dummyHediff);
        if (!wasCured) Utils.NarehateUtils.EffecterNarehateTrans(Pawn);
    }
}