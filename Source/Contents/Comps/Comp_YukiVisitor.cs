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
    // 对话进度状态
    public int interactionSteps = 1;
    public bool isTale;
    public bool isCasted;
    public bool isWorking;
    public bool isInfLevel;
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref interactionSteps, "interactionSteps", 1);
        Scribe_Values.Look(ref isCasted, "isCasted", false);
        Scribe_Values.Look(ref isWorking, "isWorking", false);
        Scribe_Values.Look(ref isTale, "isTale", false);
        Scribe_Values.Look(ref isInfLevel, "isInfLevel", false);
        
    }
    
    // 受伤时离开
    public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        base.PostPostApplyDamage(dinfo, totalDamageDealt);
        if (parent.Destroyed || !parent.Spawned) return;
        if (parent is Pawn { IsColonist: true }) return;  // 玩家阵营时跳过撤离 v1.0.6
        // 离开逻辑
        if (dinfo.Def.ExternalViolenceFor(parent))
        {
            if (ManosabaMod.Settings.debugMode) Log.Message($"[Manosaba] Yuki left due to violence: {dinfo.Def} (Comps.CompProperties_YukiVisitor.PostPostApplyDamage)");
            var letterText = LetterTextHurt + (isCasted ? null : ("\n\n" + LetterTextAttach));
            Find.LetterStack.ReceiveLetter(LetterLabelHurt, letterText, LetterDefOf.NeutralEvent, new TargetInfo(parent.Position, parent.Map));
            Notify_LeaveMap(true);
        }
        else
        {
            if (ManosabaMod.Settings.debugMode) Log.Message($"[Manosaba] Yuki left due to: injury {dinfo.Def} (Comps.CompProperties_YukiVisitor.PostPostApplyDamage)");
            var letterText = LetterTextNeutral + (isCasted ? null : ("\n\n" + LetterTextAttach));
            Find.LetterStack.ReceiveLetter(LetterLabelNeutral, letterText, LetterDefOf.NeutralEvent, new TargetInfo(parent.Position, parent.Map));
            Notify_LeaveMap();
        }
    }
    // 倒地时离开
    public override void Notify_Downed()
    {
        base.Notify_Downed();
        if (parent.Destroyed || !parent.Spawned) return;
        if (parent is Pawn { IsColonist: true }) return;  // 玩家阵营时跳过撤离 v1.0.6
        // 离开逻辑
        if (ManosabaMod.Settings.debugMode) Log.Message("[Manosaba] Yuki left due to: downed (Comps.CompProperties_YukiVisitor.PostNotify_Downed)");
        var letterText = LetterTextHurt + (isCasted ? null : ("\n\n" + LetterTextAttach));
        Find.LetterStack.ReceiveLetter(LetterLabelHurt, letterText, LetterDefOf.NeutralEvent, new TargetInfo(parent.Position, parent.Map));
        Notify_LeaveMap();
    }
    // 被捕时离开
    public override void Notify_Arrested(bool succeeded)
    {
        base.Notify_Arrested(succeeded);
        if (parent.Destroyed || !parent.Spawned) return;
        if (parent is Pawn { IsColonist: true }) return;  // 玩家阵营时跳过撤离 v1.0.6
        // 离开逻辑
        if (ManosabaMod.Settings.debugMode) Log.Message("[Manosaba] Yuki left due to: arrested (Comps.CompProperties_YukiVisitor.Notify_Arrested");
        var letterText = LetterTextNeutral + (isCasted ? null : ("\n\n" + LetterTextAttach));
        Find.LetterStack.ReceiveLetter(LetterLabelNeutral, letterText, LetterDefOf.NeutralEvent,
            new TargetInfo(parent.Position, parent.Map));
        Notify_LeaveMap();
    }
    // JobDriver引用发起对话
    public void Notify_SpokenTo(Pawn speaker)
    {
        if (parent is Pawn { IsColonist: true }) return;  // 玩家阵营时跳过撤离 v1.0.6

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
        isCasted = true;
        // 完成任务时尝试向殖民地添加 1 个【魔女因子】
        RandomSelector.TryAddDummyToRandomPawnOnMap(parent.Map, inverseTemperature: ManosabaMod.Settings.inverseTemperature);
        // 全局标记 Yuki 已来访过殖民地并散播魔女因子
        var comp = Current.Game.GetComponent<ManosabaGameComponent>();
        if (comp != null ) comp.isYukiVisited = true;
        if (!isHidden)
        {
            isWorking = false;
            if (interactionSteps == 10)
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
            isWorking = true;
            var letterLabel = "Manosaba_CompYukiVisitor_castHidden_letterLabel".Translate(pawn.LabelShort);
            var letterText = "Manosaba_CompYukiVisitor_castHidden_letterText".Translate(pawn.LabelShort);
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NegativeEvent, pawn);
        }
    }
    // JobDriver及本地引用立刻离开地图
    public void Notify_LeaveMap(bool applyMood = false)
    {
        if (parent is Pawn { IsColonist: true }) return;  // 玩家阵营时跳过撤离 v1.0.6
        YukiGeneralUtils.LeaveMapInstantly((Pawn)parent, applyMood: applyMood);
        // 状态重置
        interactionSteps = 0;
        isWorking = false;
    }
    // 右键菜单交互入口
    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
    {
        YukiGeneralUtils.CheckYukiName(selPawn);
        if (!selPawn.IsColonistPlayerControlled || interactionSteps == 0 || isWorking)
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