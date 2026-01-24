using RimWorld;
using UnityEngine;
using UranvManosaba.Contents.Utils;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_Progress : HediffCompProperties
{
    public HediffCompProperties_Progress()
    {
        compClass = typeof(HediffComp_Progress);
    }
}
public class HediffComp_Progress : HediffComp
{
    // 保存状态
    public bool isFinished;
    public bool isDisplay;
    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_Values.Look(ref isFinished, "isFinished", false);
        Scribe_Values.Look(ref isDisplay, "isDisplay", false);
    }
        
        
    private float _severityChange;
    private const int TickInterval = 500;
    private const float UpdatesPerDay = 60000f / TickInterval;
    private const float MinUpdatePerCall = 1e-7f;

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        if (isFinished) return;
        if (!Pawn.IsHashIntervalTick(TickInterval)) return;
        UpdateSeverityBasedOnMood();        // 计算隐藏进度
        TryTurnIntoNarehate();              // 转化魔女残骸
        TryMentalBreak();                   // 触发魔女因子杀意
    }
        
    // 显示当前数值 #.#E-#,#.#E-# 形式分别输出当前Severity和上一次更新变化量
    public override string CompLabelInBracketsExtra
    {
        get
        {
            var printSeverity = parent.Severity.ToString("0.#E-0");
            var printSeverityChange = _severityChange.ToString("0.#E-0");
            return $"{printSeverity}, {printSeverityChange}";
        }
    }
        
    // 根据心情调整隐藏进度
    private void UpdateSeverityBasedOnMood()
    {
        // 仅在有心情需求时更新
        if (Pawn.mindState == null || Pawn.needs?.mood == null) return;
        // 仅在清醒时更新
        if (!Pawn.Awake()) return;
        // 获取当前心情
        var mood = Pawn.needs.mood.CurLevel;
        // 获取 基础崩溃阈值 - 默认为 0.35
        var minorThreshold = Pawn.mindState is { mentalBreaker: not null } ? Pawn.mindState.mentalBreaker.BreakThresholdMinor : 0.35f;
        // 计算严重度变化
        _severityChange = GetSeverityChange(mood, minorThreshold);
        // 应用变化
        parent.Severity = Mathf.Max(0f, Mathf.Min(parent.Severity + _severityChange, 1f));
        if (parent.Severity >= 0.5f) isDisplay = true;
    }
    // 转化为魔女残骸
    private void TryTurnIntoNarehate()
    {
        // 如果已经达到 1.0 (100%), 尝试转化魔女残骸
        if (parent.Severity <= 1.0f - MinUpdatePerCall) return;
        // 所有装别放进背包
        NarehateUtils.UnequipAll(Pawn);
        // 添加 魔女残骸 Hediff
        if (!Pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffNarehate))
            Pawn.health.AddHediff(ModDefOf.UmHediffNarehate);
        if (!Pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffNarehateHidden))
            Pawn.health.AddHediff(ModDefOf.UmHediffNarehateHidden);
        // 视觉效果
        NarehateUtils.RefreshPawnGraphics(Pawn);
        NarehateUtils.EffecterNarehateTrans(Pawn);
        // 发送信件
        var label = "Manosaba_LetterLabel_TurnIntoNarehate".Translate();
        var text = "Manosaba_LetterText_TurnIntoNarehate".Translate(Pawn.Named("PAWN"));
        Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.ThreatBig, Pawn);
        // 解锁【魔女残骸】图鉴定义
        var codexDef = DefDatabase<EntityCodexEntryDef>.GetNamed("UmEntryMutant", true);
        if (codexDef is { Discovered: false })
        {
            Find.EntityCodex.SetDiscovered(codexDef);
        }
        // 修改状态
        isFinished  = true;
        isDisplay = true;
        HediffComp_HumanDummy.SetDummyShouldDisplay(Pawn); // 顺便刷新状态
        // 精神状态
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
    // 检查并触发精神崩溃
    private void TryMentalBreak()
    {
        // 动态调整精神崩溃期望周期
        var severity = parent.Severity; // 0.0 - 1.0
        if (severity <= 0.05f) return;
        var mood = Pawn.needs?.mood?.CurLevel ?? 1.0f; // 0.0 - 1.0
        var baseMtbDays = ManosabaMod.Settings.baseMtbDays; // 基础天数
        var severityFactor = Mathf.Lerp(-1.0f, 1.0f, severity); // 严重度越高, 期望天数越低
        var moodFactor = Mathf.Lerp(-1.0f, 1.0f, mood); // 心情越低, 期望天数越高
        var adjustedMtbDays = (float) (baseMtbDays * Math.Pow(4f, - severityFactor) * Math.Pow(2f, moodFactor));
            
        if (!Rand.MTBEventOccurs(adjustedMtbDays, 60000f, TickInterval)) return;
        isDisplay = true;
        ResearchUtils.UnlockResearchPrereqs();
        Pawn.mindState.mentalStateHandler.TryStartMentalState(ModDefOf.UmMentalBreakFactor);
    }
        
        
    public static float GetSeverityChange(float mood, float minorThreshold)
    {
        // 增加阈值 (默认为 0.58, 最小 0.24, 最大 0.72)
        var increaseThreshold = Mathf.Max(0.01f, Mathf.Min(minorThreshold + ManosabaMod.Settings.severityIncreaseBias/100, 0.98f)); 
        // 减少阈值 (默认为 0.66, 最小 0.32, 最大 0.80)
        var decreaseThreshold = Mathf.Max(0.02f, Mathf.Min(increaseThreshold + ManosabaMod.Settings.severityFlatBias/100, 0.99f)); 
        // 归一化范围内心情系数
        double factorRaw;
        if (mood > decreaseThreshold)
        {
            factorRaw = (mood - decreaseThreshold) / (1f - decreaseThreshold);
            factorRaw = Math.Pow(factorRaw, ManosabaMod.Settings.severityIncreaseExponent);
        }
        else if (mood < increaseThreshold)
        {
            factorRaw = (increaseThreshold - mood) / increaseThreshold;
            factorRaw = Math.Pow(factorRaw, ManosabaMod.Settings.severityDecreaseExponent);
        }
        else
        {
            factorRaw = 0f;
        }
        var factorCurved = 0.5 - 0.5 * Math.Cos(Math.PI * factorRaw); // 0..1
        var effectiveChange = 0f;
        // 实际调整
        if (mood > decreaseThreshold)  // 减少逻辑
        {
            var changeRaw = ManosabaMod.Settings.severityDecreaseFactor/100 * factorCurved / UpdatesPerDay;
            effectiveChange = -(float)Math.Max(changeRaw, MinUpdatePerCall);
        }
        else if (mood < increaseThreshold) // 增加逻辑
        {
            var changeRaw = ManosabaMod.Settings.severityIncreaseFactor/100 * factorCurved / UpdatesPerDay;
            effectiveChange = (float)Math.Max(changeRaw, MinUpdatePerCall);
        }
        return effectiveChange;
    }
}