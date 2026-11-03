using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_Countdown_Mutant : HediffCompProperties
{
    public float DraftFactor = 1f;
    public HediffCompProperties_Countdown_Mutant()
    {
        this.compClass = typeof(HediffComp_Countdown_Mutant);
    }
}

public class HediffComp_Countdown_Mutant : HediffComp
{
    private HediffCompProperties_Countdown_Mutant Props => (HediffCompProperties_Countdown_Mutant)props;
    public bool RequiresNarehateMentalState => parent.Severity <= 0.001f;
    private bool HumanRequiresNarehateMentalState => Pawn.health.hediffSet
        .GetFirstHediffOfDef(ModDefOf.UmHediffHumanDummy)?.TryGetComp<HediffComp_HumanDummy>()
        ?.RequiresNarehateMentalState == true;

    public override void CompPostPostRemoved()
    {
        base.CompPostPostRemoved();
        if (!HumanRequiresNarehateMentalState && Pawn.MentalStateDef == ModDefOf.UmMentalBreakNarehate)
        {
            Pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
        }
    }

    // 更新逻辑
    private const int TickInterval = 600; // 基础 Tick 间隔
    private const float UpdatesPerDay = 60000f / TickInterval; // 每更新频率
    private float TickChange => (Pawn.Drafted ? Props.DraftFactor : 1f) / (ManosabaMod.Settings.MutantFullCircle * UpdatesPerDay);
    public override void CompPostTickInterval(ref float severityAdjustment, int delta)
    {
        base.CompPostTickInterval(ref severityAdjustment, delta);
        // 每 600 Tick 更新倒计时
        if (Pawn.IsHashIntervalTick(TickInterval, delta))
        {
            parent.Severity = Mathf.Max(0.0001f, Mathf.Min(parent.Severity - TickChange, 1f));
        }
    }

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        // 较小间隔维护精神状态
        if (Pawn.IsHashIntervalTick(300))
        {
            switch (parent.Severity)
            {
                case > 0.001f when Pawn.MentalStateDef == ModDefOf.UmMentalBreakNarehate &&
                                  !HumanRequiresNarehateMentalState:
                    Pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
                    break;
                case <= 0.001f:
                    MaintainMentalState();
                    break;
            }
        }
    }

    // 显示进度数值
    public override string CompLabelInBracketsExtra
    {
        get
        {
            var progress = parent.Severity * ManosabaMod.Settings.MutantFullCircle;
            return progress.ToString("F1")+"HediffComp_Countdown_Mutant_Unit".Translate();
        }
    }

    // 维护精神状态
    private void MaintainMentalState()
    {
        if (Pawn.Dead || Pawn.Downed) return;
        if (Pawn.MentalStateDef == ModDefOf.UmMentalBreakNarehate) return;
        // 尝试进入魔女残骸状态
        Pawn.mindState.mentalStateHandler.TryStartMentalState(
            ModDefOf.UmMentalBreakNarehate,
            null,
            true, // forced
            true, // forceWake
            false, //causedByMood
            null, //otherPawn
            true // transitionSilently
        );
    }
}