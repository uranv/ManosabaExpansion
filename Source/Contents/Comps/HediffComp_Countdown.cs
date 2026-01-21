using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_Countdown_Mutant : HediffCompProperties
{
    public float draftFactor = 1f;
    public HediffCompProperties_Countdown_Mutant()
    {
        this.compClass = typeof(HediffComp_Countdown_Mutant);
    }
}
    
public class HediffComp_Countdown_Mutant : HediffComp
{
    private HediffCompProperties_Countdown_Mutant Props => (HediffCompProperties_Countdown_Mutant)props;

    // 更新逻辑
    private const int TickInterval = 600; // 基础 Tick 间隔
    private const float UpdatesPerDay = 60000f / TickInterval; // 每更新频率
    private float TickChange => Props.draftFactor / (ManosabaMod.Settings.mutantFullCircle * UpdatesPerDay);
    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        // 每 600 Tick 更新倒计时
        if (Pawn.IsHashIntervalTick(TickInterval))
        {
            parent.Severity = Mathf.Max(parent.Severity-TickChange, 0.0001f);
        }
        // 较小间隔维护精神状态
        if (!Pawn.IsHashIntervalTick(300)) return;
        switch (parent.Severity)
        {
            case > 0.005f when Pawn.InMentalState:
                Pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
                break;
            case <= 0.005f:
                MaintainMentalState();
                break;
        }
    }

    // 显示进度数值
    public override string CompLabelInBracketsExtra
    {
        get
        {
            var progress = parent.Severity * ManosabaMod.Settings.mutantFullCircle;
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
            null, //PawnotherPawn
            true // transitionSilently
        );
    }
}