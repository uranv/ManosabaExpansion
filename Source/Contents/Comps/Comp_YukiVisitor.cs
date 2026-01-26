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
    private int _interactionSteps = 1;
    private bool _isTale;
    // 任务进度状态
    private bool _isCasted;
    private bool _isWorking;
    private bool _isInfLevel;
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref _interactionSteps, "interactionSteps", 1);
        Scribe_Values.Look(ref _isCasted, "isCasted", false);
        Scribe_Values.Look(ref _isWorking, "isWorking", false);
        Scribe_Values.Look(ref _isTale, "isTale", false);
        Scribe_Values.Look(ref _isInfLevel, "_isInfLevel", false);
        
    }
    // 条件触发自动撤离
    private TaggedString LetterLabelHurt => "Manosaba_CompYukiVisitor_teleportLeaveHurt_letterLabel".Translate(parent.LabelShort);
    private TaggedString LetterTextHurt => "Manosaba_CompYukiVisitor_teleportLeaveHurt_letterText".Translate(parent.LabelShort);
    private TaggedString LetterTextAttach => "Manosaba_CompYukiVisitor_teleportLeaveHurt_letterTextAttach".Translate(parent.LabelShort);
    private TaggedString LetterLabelNeutral => "Manosaba_CompYukiVisitor_teleportLeaveNeutral_letterLabel".Translate(parent.LabelShort);
    private TaggedString LetterTextNeutral => "Manosaba_CompYukiVisitor_teleportLeaveNeutral_letterText".Translate(parent.LabelShort);
    // 受伤时离开
    public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        base.PostPostApplyDamage(dinfo, totalDamageDealt);
        if (parent.Destroyed || !parent.Spawned) return;
        if (dinfo.Def.ExternalViolenceFor(parent))
        {
            // 通知
            if (ManosabaMod.Settings.debugMode) Log.Message($"[Manosaba] Yuki left due to violence: {dinfo.Def} (Comps.CompProperties_YukiVisitor.PostPostApplyDamage)");
            var letterText = LetterTextHurt + (_isCasted ? null : ("\n\n" + LetterTextAttach));
            Find.LetterStack.ReceiveLetter(LetterLabelHurt, letterText, LetterDefOf.NeutralEvent, new TargetInfo(parent.Position, parent.Map));
            YukiLeaveMap(true);
        }
        else
        {
            // 通知
            if (ManosabaMod.Settings.debugMode) Log.Message($"[Manosaba] Yuki left due to: injury {dinfo.Def} (Comps.CompProperties_YukiVisitor.PostPostApplyDamage)");
            var letterText = LetterTextNeutral + (_isCasted ? null : ("\n\n" + LetterTextAttach));
            Find.LetterStack.ReceiveLetter(LetterLabelNeutral, letterText, LetterDefOf.NeutralEvent, new TargetInfo(parent.Position, parent.Map));
            YukiLeaveMap();
        }
    }
    // 倒地时离开
    public override void Notify_Downed()
    {
        if (parent.Destroyed || !parent.Spawned) return;
        // 通知
        if (ManosabaMod.Settings.debugMode) Log.Message("[Manosaba] Yuki left due to: downed (Comps.CompProperties_YukiVisitor.PostNotify_Downed)");
        var letterText = LetterTextHurt + (_isCasted ? null : ("\n\n" + LetterTextAttach));
        Find.LetterStack.ReceiveLetter(LetterLabelHurt, letterText, LetterDefOf.NeutralEvent, new TargetInfo(parent.Position, parent.Map));
        YukiLeaveMap();
        base.Notify_Downed();
    }
    // 被捕时离开
    public override void Notify_Arrested(bool succeeded)
    {
        if (parent.Destroyed || !parent.Spawned) return;
        // 通知
        if (ManosabaMod.Settings.debugMode) Log.Message("[Manosaba] Yuki left due to: arrested (Comps.CompProperties_YukiVisitor.Notify_Arrested");
        var letterText = LetterTextNeutral + (_isCasted ? null : ("\n\n" + LetterTextAttach));
        Find.LetterStack.ReceiveLetter(LetterLabelNeutral, letterText, LetterDefOf.NeutralEvent,
            new TargetInfo(parent.Position, parent.Map));
        YukiLeaveMap();
        base.Notify_Arrested(succeeded);
    }
    // 右键菜单入口
    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
    {
        YukiGeneralUtils.CheckYukiName(selPawn);
        if (!selPawn.IsColonistPlayerControlled || _interactionSteps == 0 || _isWorking)
        {
            yield break;
        }
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
    public void Notify_SpokenTo(Pawn speaker)
    {
        YukiGeneralUtils.CheckYukiName(speaker);
        var visitor = (Pawn)parent;
            
        MiscUtils.SanityCheckPawnData(visitor, "Visitor(CompYuki)");
        MiscUtils.SanityCheckPawnData(speaker, "Speaker(CompYuki)");
            
        // 加载对话内容
        LoadDialogContent(visitor);
        // 打开对话窗口
        Find.WindowStack.Add(new Dialog_NodeTree(CreateDialogTree(visitor, speaker)));
    }

    // =================================
    // 对话内容
    // =================================
    // 通用退出选项
    private TaggedString _levelChoiceQuit;
    // 最终循环选项
    private TaggedString _levelInf1;
    private TaggedString _levelInf2;
    private TaggedString _levelInf3;
    private TaggedString _levelInfChoice1;
    private TaggedString _levelInfChoiceTale;
    // 层级 1
    private TaggedString _level1;
    private TaggedString _level1Choice1;
    private TaggedString _level1Choice2;
    // 层级 2
    private TaggedString _level2;
    private TaggedString _level2Choice1;
    private TaggedString _level2Choice2;
    // 层级 2
    private TaggedString _level3;
    private TaggedString _level3Choice1;
    private TaggedString _level3Choice2;
    private TaggedString _level3Choice3;
    private TaggedString _level3Choice4;
    // 层级 4
    private TaggedString _level4;
    private TaggedString _level4Choice1;
    // 层级 5
    private TaggedString _level5;
    private TaggedString _level5Choice1;
    private TaggedString _level5Choice2;
    // 层级 6
    private TaggedString _level6;
    private TaggedString _level6Choice1;
    // 层级 10
    private TaggedString _level10;
    private TaggedString _level10Choice1;
    private TaggedString _level10Choice2;
    private TaggedString _level10Choice3;
    // 层级 11
    private TaggedString _level11;
    private TaggedString _level11Choice1;


    private void LoadDialogContent(Pawn visitor)
    {
        // 通用退出选项
        _levelChoiceQuit = "Manosaba_CompYukiVisitor_level_choice_quit".Translate(); // 通用 - 选项 3【Colonist>>Yuki】:退出对话
        // 末态分支
        _levelInf1 = "Manosaba_CompYukiVisitor_level_inf_1".Translate(visitor); // 会话 无限循环 1【Yuki>>Colonist】:
        _levelInf2 = "Manosaba_CompYukiVisitor_level_inf_2".Translate(visitor); // 会话 无限循环 2【Yuki>>Colonist】:
        _levelInf3 = "Manosaba_CompYukiVisitor_level_inf_3".Translate(visitor); // 会话 无限循环 3【Yuki>>Colonist】:
        _levelInfChoice1 = "Manosaba_CompYukiVisitor_level_inf_choice_1".Translate(); // 会话 无限循环 - 选项 1【Colonist>>Yuki】:继续
        _levelInfChoiceTale = "Manosaba_CompYukiVisitor_level_inf_choice_tale".Translate(visitor); // 会话 无限循环 - 选项 故事【Colonist>>Yuki】:讲故事

        // 层级 1
        _level1 = "Manosaba_CompYukiVisitor_level_1".Translate(visitor); // 会话 1【Yuki>>Colonist】:
        _level1Choice1 = "Manosaba_CompYukiVisitor_level_1_choice_1".Translate(); // 会话 1 - 选项 1【Colonist>>Yuki】:确认
        _level1Choice2 = "Manosaba_CompYukiVisitor_level_1_choice_2".Translate(); // 会话 1 - 选项 2【Colonist>>Yuki】:否定
        // 层级 2
        _level2 = "Manosaba_CompYukiVisitor_level_2".Translate(visitor); // 会话 1【Yuki>>Colonist】:
        _level2Choice1 = "Manosaba_CompYukiVisitor_level_2_choice_1".Translate(); // 会话 2 - 选项 1【Colonist>>Yuki】
        _level2Choice2 = "Manosaba_CompYukiVisitor_level_2_choice_2".Translate(); // 会话 2 - 选项 2【Colonist>>Yuki】
        // 层级 3
        _level3 = "Manosaba_CompYukiVisitor_level_3".Translate(visitor); // 会话 3【Yuki>>Colonist】:
        _level3Choice1 = "Manosaba_CompYukiVisitor_level_3_choice_1".Translate(); // 会话 3 - 选项 1【Colonist>>Yuki】
        _level3Choice2 = "Manosaba_CompYukiVisitor_level_3_choice_2".Translate(); // 会话 3 - 选项 2【Colonist>>Yuki】
        _level3Choice3 = "Manosaba_CompYukiVisitor_level_3_choice_3".Translate(); // 会话 3 - 选项 3【Colonist>>Yuki】
        _level3Choice4 = "Manosaba_CompYukiVisitor_level_3_choice_4".Translate(); // 会话 3 - 选项 4【Colonist>>Yuki】
        // 层级 4
        _level4 = "Manosaba_CompYukiVisitor_level_4".Translate(visitor); // 会话 4【Yuki>>Colonist】:
        _level4Choice1 = "Manosaba_CompYukiVisitor_level_4_choice_1".Translate(); // 会话 4 - 选项 1【Colonist>>Yuki】

        // 层级 5
        _level5 = "Manosaba_CompYukiVisitor_level_5".Translate(visitor); // 会话 5【Yuki>>Colonist】:
        _level5Choice1 = "Manosaba_CompYukiVisitor_level_5_choice_1".Translate(); // 会话 5 - 选项 1【Colonist>>Yuki】
        _level5Choice2 = "Manosaba_CompYukiVisitor_level_5_choice_2".Translate(); // 会话 5 - 选项 2【Colonist>>Yuki】
        // 层级 6
        _level6 = "Manosaba_CompYukiVisitor_level_6".Translate(visitor); // 会话 6【Yuki>>Colonist】:
        _level6Choice1 = "Manosaba_CompYukiVisitor_level_6_choice_1".Translate(); // 会话 6 - 选项 1【Colonist>>Yuki】
        // 层级 10
        _level10 = "Manosaba_CompYukiVisitor_level_10".Translate(visitor); // 会话 10【Yuki>>Colonist】:
        _level10Choice1 = "Manosaba_CompYukiVisitor_level_10_choice_1".Translate(); // 会话 10 - 选项 1【Colonist>>Yuki】
        _level10Choice2 = "Manosaba_CompYukiVisitor_level_10_choice_2".Translate(); // 会话 10 - 选项 2【Colonist>>Yuki】
        _level10Choice3 = "Manosaba_CompYukiVisitor_level_10_choice_3".Translate(); // 会话 10 - 选项 3【Colonist>>Yuki】
        // 层级 11
        _level11 = "Manosaba_CompYukiVisitor_level_11".Translate(visitor); // 会话 11【Yuki>>Colonist】:
        _level11Choice1 = "Manosaba_CompYukiVisitor_level_11_choice_1".Translate(); // 会话 11 - 选项 1【Colonist>>Yuki】

    }

    // 对话树
    private DiaNode CreateDialogTree(Pawn visitor, Pawn negitiator)
    {
        // =================================
        // 定义节点连接
        // =================================
        var level1 = new DiaNode(_level1);
        var level2 = new DiaNode(_level2);
        var level3 = new DiaNode(_level3);
        var level4 = new DiaNode(_level4);
        var level5 = new DiaNode(_level5+"\n\n"+GrammarUtils.GenerateTale(visitor, negitiator));
        var level5Copy = new DiaNode(_level5+"\n\n"+GrammarUtils.GenerateTale(visitor, negitiator));
        var level6 = new DiaNode(_level6);
        var level10 = new DiaNode(_level10);
        var level11 = new DiaNode(_level11);    


        var levelInf1 = new DiaNode(_levelInf1);
        var levelInf2 = new DiaNode(_levelInf2);
        var levelInf3 = new DiaNode(_levelInf3);
        var levelInf1Copy = new DiaNode(_levelInf1);
        var levelInf2Copy = new DiaNode(_levelInf2);
        var levelInf3Copy = new DiaNode(_levelInf3);

        // =================================
        // 对话层 无穷
        // =================================
        var levelInfTale = new DiaOption(_levelInfChoiceTale)
        {
            action = null,
            link = level5
        };

        // Infty 1
        var levelInf1Choice1 = new DiaOption(_levelInfChoice1);
        levelInf1Choice1.action = delegate
        {
            switch (Rand.Value)
            {
                case > 0.9f:
                    levelInf1Choice1.link = levelInf1Copy;
                    _interactionSteps = 1001;
                    break;
                case > 0.45f:
                    levelInf1Choice1.link = levelInf2;
                    _interactionSteps = 1002;
                    break;
                default:
                    levelInf1Choice1.link = levelInf3;
                    _interactionSteps = 1003;
                    break;
            }
        };
        var levelInf1Choice2 = new DiaOption(_levelChoiceQuit)
        {
            action = delegate
            {
                _interactionSteps = Rand.Value switch
                {
                    > 0.9f => 1001,
                    > 0.45f => 1002,
                    _ => 1003
                };
            },
            resolveTree = true,
        };
        levelInf1.options.Add(levelInf1Choice1);
        if (_isTale) levelInf1.options.Add(levelInfTale);
        levelInf1.options.Add(levelInf1Choice2);
        // Infty 1 Copy
        var levelInf1Choice1Copy = new DiaOption(_levelInfChoice1);
        levelInf1Choice1Copy.action = delegate
        {
            switch (Rand.Value)
            {
                case > 0.9f:
                    levelInf1Choice1Copy.link = levelInf1;
                    _interactionSteps = 1001;
                    break;
                case > 0.45f:
                    levelInf1Choice1Copy.link = levelInf2;
                    _interactionSteps = 1002;
                    break;
                default:
                    levelInf1Choice1Copy.link = levelInf3;
                    _interactionSteps = 1003;
                    break;
            }
        };
        var levelInf1Choice2Copy = new DiaOption(_levelChoiceQuit)
        {
            action = delegate
            {
                _interactionSteps = Rand.Value switch
                {
                    > 0.9f => 1001,
                    > 0.45f => 1002,
                    _ => 1003
                };
            },
            resolveTree = true,
        };
        levelInf1Copy.options.Add(levelInf1Choice1Copy);
        if (_isTale) levelInf1Copy.options.Add(levelInfTale);
        levelInf1Copy.options.Add(levelInf1Choice2Copy);

        // Infty 2
        var levelInf2Choice1 = new DiaOption(_levelInfChoice1);
        levelInf2Choice1.action = delegate
        {
            switch (Rand.Value)
            {
                case > 0.9f:
                    levelInf2Choice1.link = levelInf2Copy;
                    _interactionSteps = 1002;
                    break;
                case > 0.45f:
                    levelInf2Choice1.link = levelInf1;
                    _interactionSteps = 1001;
                    break;
                default:
                    levelInf2Choice1.link = levelInf3;
                    _interactionSteps = 1003;
                    break;
            }
        };
        var levelInf2Choice2 = new DiaOption(_levelChoiceQuit)
        {
            action = delegate
            {
                _interactionSteps = Rand.Value switch
                {
                    > 0.9f => 1002,
                    > 0.45f => 1001,
                    _ => 1003
                };
            },
            resolveTree = true,
        };
        levelInf2.options.Add(levelInf2Choice1);
        if (_isTale) levelInf2.options.Add(levelInfTale);
        levelInf2.options.Add(levelInf2Choice2);
        // Infty 2 Copy
        var levelInf2Choice1Copy = new DiaOption(_levelInfChoice1);
        levelInf2Choice1Copy.action = delegate
        {
            switch (Rand.Value)
            {
                case > 0.9f:
                    levelInf2Choice1Copy.link = levelInf2;
                    _interactionSteps = 1002;
                    break;
                case > 0.45f:
                    levelInf2Choice1Copy.link = levelInf1;
                    _interactionSteps = 1001;
                    break;
                default:
                    levelInf2Choice1Copy.link = levelInf3;
                    _interactionSteps = 1003;
                    break;
            }
        };
        var levelInf2Choice2Copy = new DiaOption(_levelChoiceQuit)
        {
            action = delegate
            {
                _interactionSteps = Rand.Value switch
                {
                    > 0.9f => 1002,
                    > 0.45f => 1001,
                    _ => 1003
                };
            },
            resolveTree = true,
        };
        levelInf2Copy.options.Add(levelInf2Choice1Copy);
        if (_isTale) levelInf2Copy.options.Add(levelInfTale);
        levelInf2Copy.options.Add(levelInf2Choice2Copy);

        // Infty 3
        var levelInf3Choice1 = new DiaOption(_levelInfChoice1);
        levelInf3Choice1.action = delegate
        {
            switch (Rand.Value)
            {
                case > 0.9f:
                    levelInf3Choice1.link = levelInf3Copy;
                    _interactionSteps = 1003;
                    break;
                case > 0.45f:
                    levelInf3Choice1.link = levelInf1;
                    _interactionSteps = 1001;
                    break;
                default:
                    levelInf3Choice1.link = levelInf2;
                    _interactionSteps = 1002;
                    break;
            }
        };
        var levelInf3Choice2 = new DiaOption(_levelChoiceQuit)
        {
            action = delegate
            {
                _interactionSteps = Rand.Value switch
                {
                    > 0.9f => 1003,
                    > 0.45f => 1001,
                    _ => 1002
                };
            },
            resolveTree = true,
        };
        levelInf3.options.Add(levelInf3Choice1);
        if (_isTale) levelInf3.options.Add(levelInfTale);
        levelInf3.options.Add(levelInf3Choice2);
        // Infty 3 Copy
        var levelInf3Choice1Copy = new DiaOption(_levelInfChoice1);
        levelInf3Choice1Copy.action = delegate
        {
            switch (Rand.Value)
            {
                case > 0.9f:
                    levelInf3Choice1Copy.link = levelInf3;
                    _interactionSteps = 1003;
                    break;
                case > 0.45f:
                    levelInf3Choice1Copy.link = levelInf1;
                    _interactionSteps = 1001;
                    break;
                default:
                    levelInf3Choice1Copy.link = levelInf2;
                    _interactionSteps = 1002;
                    break;
            }
        };
        var levelInf3Choice2Copy = new DiaOption(_levelChoiceQuit)
        {
            action = delegate
            {
                _interactionSteps = Rand.Value switch
                {
                    > 0.9f => 1003,
                    > 0.45f => 1001,
                    _ => 1002
                };
            },
            resolveTree = true,
        };
        levelInf3Copy.options.Add(levelInf3Choice1Copy);
        if (_isTale) levelInf3Copy.options.Add(levelInfTale);
        levelInf3Copy.options.Add(levelInf3Choice2Copy);








        //---------------------------------------------
        // 对话层 11:
        // 雪: 完成任务分支；
        //    O1: 继续对话 -> 对话层: 1001, 进入循环分支
        //    O2: 结束对话 -> 后续进入循环分支
        //---------------------------------------------
        var level11Choice1 = new DiaOption(_level11Choice1)
        {
            action = delegate {
                _interactionSteps = 1001;
                _isInfLevel = true;
            },
            link = levelInf1,
        };
        var level11Choice2 = new DiaOption(_levelChoiceQuit)
        {
            action = delegate {
                _interactionSteps = 1001;
                _isInfLevel = true;
            },
            resolveTree = true,
        };

        level11.options.Add(level11Choice1);
        level11.options.Add(level11Choice2);
        //---------------------------------------------
        // 对话层 10:
        // 雪: 完成任务后, 选择奖励；
        //    O1: 奖励1: 科技点 -> 对话层: 11, 完成后继续
        //    O2: 奖励2: 白银 -> 对话层: 11, 完成后继续
        //    O3: 奖励3: 讲故事 -> 对话层: 5, 可以多听故事
        //---------------------------------------------
        var level10Choice1 = new DiaOption(_level10Choice1)
        {
            action = delegate { YukiVisitorUtils.AddResearchPointsSafe(visitor, Rand.Range(800,1200)); _interactionSteps = 11; },
            link = level11,
        };
        var level10Choice2 = new DiaOption(_level10Choice2)
        {
            action = delegate { YukiVisitorUtils.SpawnRandomRewards(visitor, Rand.Range(800,1200)); _interactionSteps = 11; },
            link = level11,
        };
        var level10Choice3 = new DiaOption(_level10Choice3)
        {
            action = delegate { _interactionSteps = 5; _isTale = true; },
            link = level5,
        };
        level10.options.Add(level10Choice1);
        level10.options.Add(level10Choice2);
        level10.options.Add(level10Choice3);
        //---------------------------------------------
        // 对话层 6:
        // 雪: 讲故事分支, 讲累了；
        //    O1: 确认完成 -> 调用执行, 对话层: 11, 完成后继续
        //---------------------------------------------
        var level6Choice1 = new DiaOption(_level6Choice1){resolveTree = true,};
        level6Choice1.action = delegate {
            if (_isInfLevel)
            {
                level6Choice1.resolveTree = true;
            }
            else if (!_isCasted)
            {
                _interactionSteps = 11;
                _isWorking = true;
                level6Choice1.resolveTree = true;
                YukiVisitorCast(visitor);
            }
            else
            {
                level6Choice1.link = level11;
                _interactionSteps = 11;
            }
        };
        level6.options.Add(level6Choice1);
        //---------------------------------------------
        // 对话层 5: 辅助层
        // 雪: 选择讲故事奖励特殊；
        //    O1: 再听一个故事 -> （概率95%）对话层: 5, 自循环
        //                      （概率5%）对话层: 6, 讲累了
        //    O2: 我听够了-> （尚未执行）调用执行, 对话层: 11, 完成后继续
        //                  （已完成）对话层11, 奖励后对话
        //---------------------------------------------
        var level5Choice1Copy = new DiaOption(_level5Choice1);
        level5Choice1Copy.action = delegate {
            _interactionSteps = _isInfLevel ? _interactionSteps : 5;
            level5.text = Rand.Value > 0.80f ? GrammarUtils.GenerateStory(visitor, negitiator) : GrammarUtils.GenerateTale(visitor, negitiator);
            level5Choice1Copy.link = Rand.Value > 0.05f ? level5 : level6;
        };
        var level5Choice2Copy = new DiaOption(_level5Choice2);
        level5Choice2Copy.action = delegate {
            if (_isInfLevel)
            {
                level5Choice2Copy.resolveTree = true;
            }
            else if (!_isCasted)
            {
                level5Choice2Copy.resolveTree = true;
                _interactionSteps = 11;
                _isWorking = true;
                YukiVisitorCast(visitor);

            }
            else
            {
                level5Choice2Copy.link = level11;
                _interactionSteps = 11;
            }
        };
        level5Copy.options.Add(level5Choice1Copy);
        level5Copy.options.Add(level5Choice2Copy);
        //---------------------------------------------
        // 对话层 5:
        // 雪: 选择讲故事奖励特殊；
        //    O1: 再听一个故事 -> （概率95%）对话层: 5copy, 自循环
        //                      （概率5%）对话层: 6, 讲累了
        //    O2: 我听够了-> （尚未执行）调用执行, 对话层: 11, 完成后继续
        //                  （已完成）对话层11, 奖励后对话
        //---------------------------------------------
        var level5Choice1 = new DiaOption(_level5Choice1);
        level5Choice1.action = delegate {
            _interactionSteps = _isInfLevel ? _interactionSteps : 5;
            level5Copy.text = Rand.Value > 0.80f ? GrammarUtils.GenerateStory(visitor, negitiator) : GrammarUtils.GenerateTale(visitor, negitiator);
            level5Choice1.link = Rand.Value > 0.05f ? level5Copy : level6;
        };
        var level5Choice2 = new DiaOption(_level5Choice2)
        {
            resolveTree = true
        };
        level5Choice2.action = delegate {
            if (_isInfLevel)
            {
                level5Choice2.resolveTree = true;
            }
            else if (!_isCasted)
            {
                level5Choice2.resolveTree = true;
                _interactionSteps = 11;
                _isWorking = true;
                YukiVisitorCast(visitor);
            }
            else
            {
                level5Choice2.link = level11;
                _interactionSteps = 11;
            }
        };
        level5.options.Add(level5Choice1);
        level5.options.Add(level5Choice2);
        //---------------------------------------------
        // 对话层 4:
        // 雪: 拒绝分支；
        //    O1: 确认完成 -> 对话层: 0, 结束分支
        //---------------------------------------------
        var level4Choice1 = new DiaOption(_level4Choice1)
        {
            action = delegate { YukiVisitorCastHidden(visitor); _interactionSteps = 0; },
            resolveTree = true,
        };
        level4.options.Add(level4Choice1);
        //---------------------------------------------
        // 对话层 3:
        // 雪: 选择奖励, 而后执行任务；
        //    O1: 奖励1: 科技点 -> 调用执行, 对话层: 11, 完成后继续
        //    O2: 奖励2: 白银 -> 调用执行, 对话层: 11, 完成后继续
        //    O3: 奖励3: 讲故事 -> 对话层: 5, 可以多听故事
        //    O4: 转念一想, 拒绝 -> 对话层: 4
        //---------------------------------------------
        var level3Choice1 = new DiaOption(_level3Choice1)
        {
            action = delegate { YukiVisitorUtils.AddResearchPointsSafe(visitor, Rand.Range(350,650)); YukiVisitorCast(visitor); _interactionSteps = 11; _isWorking = true; },
            resolveTree = true,
        };
        var level3Choice2 = new DiaOption(_level3Choice2)
        {
            action = delegate { YukiVisitorUtils.SpawnRandomRewards(visitor, Rand.Range(350,650)); YukiVisitorCast(visitor); _interactionSteps = 11; _isWorking = true; },
            resolveTree = true,
        };
        var level3Choice3 = new DiaOption(_level3Choice3)
        {
            action = delegate { _interactionSteps = 5; _isTale = true; },
            link = level5,
        };
        var level3Choice4 = new DiaOption(_level3Choice4)
        {
            action = delegate { _interactionSteps = 4; },
            link = level4,
        };

        level3.options.Add(level3Choice1);
        level3.options.Add(level3Choice2);
        level3.options.Add(level3Choice3);
        level3.options.Add(level3Choice4);
        //---------------------------------------------
        // 对话层 2:
        // 雪: 再次请求执行, 先给奖励；
        //    O1: 同意 -> 对话层: 3
        //    O2: 拒绝 -> 对话层: 4
        //    O3: 推迟 -> 无动作, 退出对话
        //---------------------------------------------
        var level2Choice1 = new DiaOption(_level2Choice1)
        {
            action = delegate { _interactionSteps = 3; },
            link = level3,
        };
        var level2Choice2 = new DiaOption(_level2Choice2)
        {
            action = delegate { _interactionSteps = 4; },
            link = level4,
        };
        var level2Choice3 = new DiaOption(_levelChoiceQuit)
        {
            action = null,
            resolveTree = true,
        };
        level2.options.Add(level2Choice1);
        level2.options.Add(level2Choice2);
        level2.options.Add(level2Choice3);
        //---------------------------------------------
        // 对话层 1:
        // 雪: 请求执行；
        //    O1: 同意 -> 调用执行, 对话层: 10
        //    O2: 拒绝 -> 对话层: 2
        //    O3: 推迟 -> 无动作, 退出对话
        //---------------------------------------------
        var level1Choice1 = new DiaOption(_level1Choice1)
        {
            action = delegate { YukiVisitorCast(visitor); _interactionSteps = 10; _isWorking = true; },
            resolveTree = true,
        };
        var level1Choice2 = new DiaOption(_level1Choice2)
        {
            action = delegate { _interactionSteps = 2; },
            link = level2,
        };
        var level1Choice3 = new DiaOption(_levelChoiceQuit)
        {
            action = null,
            resolveTree = true,
        };
        level1.options.Add(level1Choice1);
        level1.options.Add(level1Choice2);
        level1.options.Add(level1Choice3);
        // =================================
        // 决定入口点 (Return Root)
        // =================================
        if (_interactionSteps == 1)
        {
            return level1;
        }
        else if (_interactionSteps == 2)
        {
            return level2;
        }
        else if (_interactionSteps == 3)
        {
            return level3;
        }
        else if (_interactionSteps == 4)
        {
            return level4;
        }
        else if (_interactionSteps == 5)
        {
            return level5;
        }
        else if (_interactionSteps == 6)
        {
            return level6;
        }
        else if (_interactionSteps == 10)
        {
            return level10;
        }
        else if (_interactionSteps == 11)
        {
            return level11;
        }
        else if (_interactionSteps == 1001)
        {
            return levelInf1;
        }
        else if (_interactionSteps == 1002)
        {
            return levelInf2;
        }
        else if (_interactionSteps == 1003)
        {
            return levelInf3;
        }
        else
        {
            return levelInf1;
        }
    }

    // --- 辅助方法 ---
    public void Notify_isFinished(Pawn pawn, bool isHidden = false)
    {
        _isCasted = true;
        // 完成任务时尝试向殖民地添加 1 个【魔女因子】
        RandomSelector.TryAddDummyToRandomPawnOnMap(parent.Map, inverseTemperature: ManosabaMod.Settings.inverseTemperature);
        // 全局标记 Yuki 已来访过殖民地并散播魔女因子
        var comp = Current.Game.GetComponent<ManosabaGameComponent>();
        if (comp != null ) comp.isYukiVisited = true;
        if (!isHidden)
        {
            _isWorking = false;
            // 发信 (原本在 JoinColony 里的信现在移到这里发, 因为只有到了才算完成)
            if (_interactionSteps == 10)
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
            _isWorking = true;
            var letterLabel = "Manosaba_CompYukiVisitor_castHidden_letterLabel".Translate(pawn.LabelShort);
            var letterText = "Manosaba_CompYukiVisitor_castHidden_letterText".Translate(pawn.LabelShort);
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NegativeEvent, pawn);
        }
    }
    private void YukiVisitorCast(Pawn p)
    {
        if (p?.Map == null || !p.Spawned)
        {
            Log.Error("[Manosaba] Pawn.Map is null or Pawn is not spawned (Comps.Comp_YukiVisitor.YukiVisitorCast)");
            return;
        }
        // 寻找目标点: 聚会点->地图中心->中心附近点
        IntVec3 targetCell;
        if (p.Map.gatherSpotLister.activeSpots.Count > 0)
        {
            var spot = p.Map.gatherSpotLister.activeSpots.RandomElement();
            targetCell = spot.parent.InteractionCell; 
        }
        else
        {
            targetCell = p.Map.Center;
            if (!targetCell.Walkable(p.Map))
            {
                targetCell = CellFinder.RandomClosewalkCellNear(targetCell, p.Map, 10);
            }
        }

        // 创建 Job
        var job = JobMaker.MakeJob(ModDefOf.UmJobYukiVisitorCast, targetCell);
        job.playerForced = true;
        p.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        //Log.Warning("[Manosaba] YukiVisitorCast job generated");

        // 锁定状态
        _isWorking = true; 
    }

    private void YukiVisitorCastHidden(Pawn p)
    {
        if (p?.Map == null || !p.Spawned)
        {
            Log.Error("[Manosaba] Pawn.Map is null or Pawn is not spawned (Comps.Comp_YukiVisitor.YukiVisitorCastHidden)");
            return;
        }
        _interactionSteps = 0;
        // 寻找目标点: 聚会点->地图中心->中心附近点
        var map = p.Map;
        IntVec3 centerDest;
        if (map.gatherSpotLister.activeSpots.Count > 0)
        {
            var spot = map.gatherSpotLister.activeSpots.RandomElement();
            centerDest = spot.parent.InteractionCell; 
        }
        else
        {
            centerDest = map.Center;
            if (!centerDest.Walkable(map))
            {
                centerDest = CellFinder.RandomClosewalkCellNear(centerDest, map, 10);
            }
        }

        var foundEdge = CellFinder.TryFindRandomEdgeCellWith(
            c => c.Walkable(map) && p.CanReach(c, PathEndMode.OnCell, Danger.Deadly),
            map,
            0f,
            out var edgeDest
        );
        if (!foundEdge)
        {
            edgeDest = CellFinder.RandomEdgeCell(map);
        }
        // 创建 Job
        var job = JobMaker.MakeJob(ModDefOf.UmJobYukiVisitorCastHidden, edgeDest, centerDest);
        job.playerForced = true;
        p.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        //Log.Warning("[Manosaba] YukiVisitorCastHidden job generated");
        // 锁定状态
        _isWorking = true;
    }



    // 瞬移离开
    public void YukiLeaveMap(bool applyMood = false)
    {
        var p = (Pawn)parent;
        YukiGeneralUtils.LeaveMapInstantly(p, applyMood: applyMood);
        // 状态重置
        _interactionSteps = 0;
        _isWorking = false;
    }
}