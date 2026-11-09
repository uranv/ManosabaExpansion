using RimWorld;
using UranvManosaba.Contents.Utils;
using Verse;
using Verse.AI;

namespace UranvManosaba.Contents.Comps;

public class CompProperties_YukiVisitor : CompProperties
{
    public CompProperties_YukiVisitor()
    {
        compClass = typeof(Comp_YukiVisitor);
    }
}
public class Comp_YukiVisitor : ThingComp
{
    // 对话进度：0 隐藏施法及离场，10 施法后领奖，11 已选奖励及后续对话。
    public int InteractionSteps = 1;
    public bool IsTale;
    public bool IsCasted;
    public bool IsWorking;
    public bool IsInfLevel;
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref InteractionSteps, "interactionSteps", 1);
        Scribe_Values.Look(ref IsCasted, "isCasted", false);
        Scribe_Values.Look(ref IsWorking, "isWorking", false);
        Scribe_Values.Look(ref IsTale, "isTale", false);
        Scribe_Values.Look(ref IsInfLevel, "isInfLevel", false);

    }

    private bool HasCastJob
    {
        get
        {
            var pawn = (Pawn)parent;
            return (pawn.jobs.curDriver?.ended != true &&
                    (pawn.CurJobDef == ModDefOf.UmJobYukiVisitorCast || pawn.CurJobDef == ModDefOf.UmJobYukiVisitorCastHidden)) ||
                   pawn.jobs.jobQueue.Any(queued => queued.job.def == ModDefOf.UmJobYukiVisitorCast ||
                                                    queued.job.def == ModDefOf.UmJobYukiVisitorCastHidden);
        }
    }

    public void StartCast(Job job)
    {
        if (IsCasted || HasCastJob)
        {
            RefreshCastState();
            return;
        }
        job.playerForced = true;
        IsWorking = ((Pawn)parent).jobs.TryTakeOrderedJob(job, JobTag.Misc) && HasCastJob;
    }

    public void RefreshCastState()
    {
        IsWorking = HasCastJob;
        if (IsWorking) return;

        var pawn = (Pawn)parent;
        var invisibleHediff = pawn.health.hediffSet.GetFirstHediffOfDef(ModDefOf.UmHediffYukiVisitorInvisibility);
        if (invisibleHediff != null) pawn.health.RemoveHediff(invisibleHediff);
    }

    public override void CompTick()
    {
        base.CompTick();
        if (parent is not Pawn { Spawned: true, IsColonist: false } pawn || !pawn.IsHashIntervalTick(60)) return;

        RefreshCastState();
        // 任务清理回调仍在 JobTracker 内部运行，离场留到后续 tick。
        if (!IsWorking && InteractionSteps == 0) Notify_LeaveMap();
    }

    // 受伤时离开
    public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        base.PostPostApplyDamage(dinfo, totalDamageDealt);
        // 离场推迟到 tick 结束，已登记时不再重复发信
        if (parent.Destroyed || !parent.Spawned || YukiGeneralUtils.IsLeaving((Pawn)parent)) return;
        if (parent is Pawn { IsColonist: true }) return;  // 玩家阵营时跳过撤离 v1.0.6
        // 离开逻辑
        if (dinfo.Def.ExternalViolenceFor(parent))
        {
            if (ManosabaMod.Settings.DebugMode) Log.Message($"[Manosaba] Yuki left due to violence: {dinfo.Def} (Comps.CompProperties_YukiVisitor.PostPostApplyDamage)");
            var letterText = LetterTextHurt + (IsCasted ? null : ("\n\n" + LetterTextAttach));
            Find.LetterStack.ReceiveLetter(LetterLabelHurt, letterText, LetterDefOf.NeutralEvent, new TargetInfo(parent.Position, parent.Map));
            Notify_LeaveMap(true);
        }
        else
        {
            if (ManosabaMod.Settings.DebugMode) Log.Message($"[Manosaba] Yuki left due to: injury {dinfo.Def} (Comps.CompProperties_YukiVisitor.PostPostApplyDamage)");
            var letterText = LetterTextNeutral + (IsCasted ? null : ("\n\n" + LetterTextAttach));
            Find.LetterStack.ReceiveLetter(LetterLabelNeutral, letterText, LetterDefOf.NeutralEvent, new TargetInfo(parent.Position, parent.Map));
            Notify_LeaveMap();
        }
    }
    // 倒地时离开
    public override void Notify_Downed()
    {
        base.Notify_Downed();
        if (parent.Destroyed || !parent.Spawned || YukiGeneralUtils.IsLeaving((Pawn)parent)) return;
        if (parent is Pawn { IsColonist: true }) return;  // 玩家阵营时跳过撤离 v1.0.6
        // 离开逻辑
        if (ManosabaMod.Settings.DebugMode) Log.Message("[Manosaba] Yuki left due to: downed (Comps.CompProperties_YukiVisitor.PostNotify_Downed)");
        var letterText = LetterTextHurt + (IsCasted ? null : ("\n\n" + LetterTextAttach));
        Find.LetterStack.ReceiveLetter(LetterLabelHurt, letterText, LetterDefOf.NeutralEvent, new TargetInfo(parent.Position, parent.Map));
        Notify_LeaveMap();
    }
    // 被捕时离开
    public override void Notify_Arrested(bool succeeded)
    {
        base.Notify_Arrested(succeeded);
        if (parent.Destroyed || !parent.Spawned || YukiGeneralUtils.IsLeaving((Pawn)parent)) return;
        if (parent is Pawn { IsColonist: true }) return;  // 玩家阵营时跳过撤离 v1.0.6
        // 离开逻辑
        if (ManosabaMod.Settings.DebugMode) Log.Message("[Manosaba] Yuki left due to: arrested (Comps.CompProperties_YukiVisitor.Notify_Arrested");
        var letterText = LetterTextNeutral + (IsCasted ? null : ("\n\n" + LetterTextAttach));
        Find.LetterStack.ReceiveLetter(LetterLabelNeutral, letterText, LetterDefOf.NeutralEvent,
            new TargetInfo(parent.Position, parent.Map));
        Notify_LeaveMap();
    }
    // JobDriver引用发起对话
    public void Notify_SpokenTo(Pawn speaker)
    {
        if (parent is Pawn { IsColonist: true }) return;  // 玩家阵营时跳过撤离 v1.0.6

        RefreshCastState();
        if (IsWorking || InteractionSteps == 0) return;

        YukiGeneralUtils.CheckYukiName(speaker);
        var visitor = (Pawn)parent;
        MiscUtils.SanityCheckPawnData(visitor, "Visitor(CompYuki)");
        MiscUtils.SanityCheckPawnData(speaker, "Speaker(CompYuki)");
        // 打开对话窗口
        Find.WindowStack.Add(
            new Dialog_NodeTree(YukiVisitorDialogUtils.CreateDialogTree(speaker, LoadDialogues, this))
            );
    }
    // JobDriver引用完成任务
    public void Notify_Finished(Pawn pawn, bool isHidden = false)
    {
        if (IsCasted) return;
        IsCasted = true;
        // 完成任务时尝试向殖民地添加 1 个【魔女因子】
        RandomSelector.TryAddDummyToRandomPawnOnMap(parent.Map, inverseTemperature: ManosabaMod.Settings.InverseTemperature);
        // 全局标记 Yuki 已来访过殖民地并散播魔女因子
        var comp = Current.Game.GetComponent<ManosabaGameComponent>();
        if (comp != null ) comp.IsYukiVisited = true;
        if (!isHidden)
        {
            IsWorking = false;
            if (InteractionSteps == 10)
            {
                var letterLabel = "Manosaba_CompYukiVisitor_castBefore_letterLabel".Translate(pawn.LabelShort);
                var letterText = "Manosaba_CompYukiVisitor_castBefore_letterText".Translate(pawn.LabelShort);
                Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent, pawn);
            }
            else
            {
                var letterLabel = "Manosaba_CompYukiVisitor_castAfter_letterLabel".Translate(pawn.LabelShort);
                var letterText = "Manosaba_CompYukiVisitor_castAfter_letterText".Translate(pawn.LabelShort);
                Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent, pawn);
            }
        }
        else
        {
            IsWorking = true;
            var letterLabel = "Manosaba_CompYukiVisitor_castHidden_letterLabel".Translate(pawn.LabelShort);
            var letterText = "Manosaba_CompYukiVisitor_castHidden_letterText".Translate(pawn.LabelShort);
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NegativeEvent, pawn);
        }
    }
    // JobDriver及本地引用登记离场，当前 tick 结束后撤离
    public void Notify_LeaveMap(bool applyMood = false)
    {
        if (parent is Pawn { IsColonist: true }) return;  // 玩家阵营时跳过撤离 v1.0.6
        YukiGeneralUtils.LeaveMapInstantly((Pawn)parent, applyMood: applyMood);
        // 状态重置
        InteractionSteps = 0;
        IsWorking = false;
    }
    // 右键菜单交互入口
    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
    {
        RefreshCastState();
        YukiGeneralUtils.CheckYukiName(selPawn);
        if (!selPawn.IsColonistPlayerControlled || InteractionSteps == 0 || IsWorking)
        {
            yield break;
        }
        if (parent is Pawn { IsColonist: true }) yield break;  // 玩家阵营时跳过撤离 v1.0.6

        var visitor = (Pawn)parent;
        if (!selPawn.CanReach(visitor, PathEndMode.Touch, Danger.Deadly))
        {
            yield return new FloatMenuOption("Manosaba_CompYukiVisitor_CannotTalkTo".Translate(visitor.LabelShort) + ": " + "Manosaba_CompYukiVisitor_NoPath".Translate().CapitalizeFirst(), null);
            yield break;
        }
        if (!selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
        {
            yield return new FloatMenuOption("Manosaba_CompYukiVisitor_CannotTalkTo".Translate(visitor.LabelShort) + ": " + "Manosaba_CompYukiVisitor_Incapable".Translate().CapitalizeFirst(), null);
            yield break;
        }
        if (visitor.Downed || !visitor.Awake() || visitor.InMentalState || !visitor.Spawned)
        {
            yield return new FloatMenuOption("Manosaba_CompYukiVisitor_CannotTalkTo".Translate(visitor.LabelShort) + ": " + "Manosaba_CompYukiVisitor_IsBusy".Translate(visitor.LabelShort), null);
            yield break;
        }
        yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Manosaba_CompYukiVisitor_TalkTo".Translate(visitor.LabelShort), delegate
        {
            MiscUtils.SanityCheckPawnData(visitor, "Visitor(CompYuki)");
            var job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("UmJobTalkToYuki"), visitor);
            job.playerForced = true;
            selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }), selPawn, visitor);
    }


    // 发信文本
    private TaggedString LetterLabelHurt => "Manosaba_CompYukiVisitor_teleportLeaveHurt_letterLabel".Translate(parent.LabelShort);
    private TaggedString LetterTextHurt => "Manosaba_CompYukiVisitor_teleportLeaveHurt_letterText".Translate(parent.LabelShort);
    private TaggedString LetterTextAttach => "Manosaba_CompYukiVisitor_teleportLeaveHurt_letterTextAttach".Translate(parent.LabelShort);
    private TaggedString LetterLabelNeutral => "Manosaba_CompYukiVisitor_teleportLeaveNeutral_letterLabel".Translate(parent.LabelShort);
    private TaggedString LetterTextNeutral => "Manosaba_CompYukiVisitor_teleportLeaveNeutral_letterText".Translate(parent.LabelShort);
    // 对话文本
    private YukiVisitorDialogues _cachedDialogues;
    private YukiVisitorDialogues LoadDialogues
    {
        get
        {
            _cachedDialogues ??= new YukiVisitorDialogues((Pawn)parent);
            return _cachedDialogues;
        }
    }




}