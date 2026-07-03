using UranvManosaba.Contents.Comps;
using Verse;
using Verse.AI;

namespace UranvManosaba.Contents.Utils;

public static class YukiVisitorDialogUtils
{
    // 对话树
    public static DiaNode CreateDialogTree(Pawn negotiator, YukiVisitorDialogues dialog, Comp_YukiVisitor comp)
    {
        if (comp.parent is not Pawn visitor)
        {
            Log.Error("[Manosaba] comp.parent is not Pawn (YukiVisitorDialogUtils.CreateDialogTree)");
            return null;
        }
        // ===================================
        // ============== Nodes ==============
        // ===================================
        
        var level1 = new DiaNode(dialog.Level1);
        var level2 = new DiaNode(dialog.Level2);
        var level3 = new DiaNode(dialog.Level3);
        var level4 = new DiaNode(dialog.Level4);
        var level5 = new DiaNode(dialog.Level5+"\n\n"+GrammarUtils.GenerateTale(visitor, negotiator));
        var level5Copy = new DiaNode(dialog.Level5+"\n\n"+GrammarUtils.GenerateTale(visitor, negotiator));
        var level6 = new DiaNode(dialog.Level6);
        var level10 = new DiaNode(dialog.Level10);
        var level11 = new DiaNode(dialog.Level11);    
        
        var levelInf1 = new DiaNode(dialog.LevelInf1);
        var levelInf2 = new DiaNode(dialog.LevelInf2);
        var levelInf3 = new DiaNode(dialog.LevelInf3);
        var levelInf1Copy = new DiaNode(dialog.LevelInf1);
        var levelInf2Copy = new DiaNode(dialog.LevelInf2);
        var levelInf3Copy = new DiaNode(dialog.LevelInf3);
        
        // ===================================
        // ======== Infinite Options =========
        // ===================================
        
        // 无限选项故事
        var levelInfTale = new DiaOption(dialog.LevelInfChoiceTale)
        {
            action = null,
            link = level5
        };
        // 无限选项1-1
        var levelInf1Choice1 = new DiaOption(dialog.LevelInfChoice1);
        levelInf1Choice1.action = delegate
        {
            switch (Rand.Value)
            {
                case > 0.9f:
                    levelInf1Choice1.link = levelInf1Copy;
                    comp.interactionSteps = 1001;
                    break;
                case > 0.45f:
                    levelInf1Choice1.link = levelInf2;
                    comp.interactionSteps = 1002;
                    break;
                default:
                    levelInf1Choice1.link = levelInf3;
                    comp.interactionSteps = 1003;
                    break;
            }
        };
        // 无限选项1-2
        var levelInf1Choice2 = new DiaOption(dialog.LevelChoiceQuit)
        {
            action = delegate
            {
                comp.interactionSteps = Rand.Value switch
                {
                    > 0.9f => 1001,
                    > 0.45f => 1002,
                    _ => 1003
                };
            },
            resolveTree = true,
        };
        // 无限选项1-1复制
        var levelInf1Choice1Copy = new DiaOption(dialog.LevelInfChoice1);
        levelInf1Choice1Copy.action = delegate
        {
            switch (Rand.Value)
            {
                case > 0.9f:
                    levelInf1Choice1Copy.link = levelInf1;
                    comp.interactionSteps = 1001;
                    break;
                case > 0.45f:
                    levelInf1Choice1Copy.link = levelInf2;
                    comp.interactionSteps = 1002;
                    break;
                default:
                    levelInf1Choice1Copy.link = levelInf3;
                    comp.interactionSteps = 1003;
                    break;
            }
        };
        // 无限选项1-2复制
        var levelInf1Choice2Copy = new DiaOption(dialog.LevelChoiceQuit)
        {
            action = delegate
            {
                comp.interactionSteps = Rand.Value switch
                {
                    > 0.9f => 1001,
                    > 0.45f => 1002,
                    _ => 1003
                };
            },
            resolveTree = true,
        };
        // 无限选项2-1
        var levelInf2Choice1 = new DiaOption(dialog.LevelInfChoice1);
        levelInf2Choice1.action = delegate
        {
            switch (Rand.Value)
            {
                case > 0.9f:
                    levelInf2Choice1.link = levelInf2Copy;
                    comp.interactionSteps = 1002;
                    break;
                case > 0.45f:
                    levelInf2Choice1.link = levelInf1;
                    comp.interactionSteps = 1001;
                    break;
                default:
                    levelInf2Choice1.link = levelInf3;
                    comp.interactionSteps = 1003;
                    break;
            }
        };
        // 无限选项2-2
        var levelInf2Choice2 = new DiaOption(dialog.LevelChoiceQuit)
        {
            action = delegate
            {
                comp.interactionSteps = Rand.Value switch
                {
                    > 0.9f => 1002,
                    > 0.45f => 1001,
                    _ => 1003
                };
            },
            resolveTree = true,
        };
        // 无限选项2-1复制
        var levelInf2Choice1Copy = new DiaOption(dialog.LevelInfChoice1);
        levelInf2Choice1Copy.action = delegate
        {
            switch (Rand.Value)
            {
                case > 0.9f:
                    levelInf2Choice1Copy.link = levelInf2;
                    comp.interactionSteps = 1002;
                    break;
                case > 0.45f:
                    levelInf2Choice1Copy.link = levelInf1;
                    comp.interactionSteps = 1001;
                    break;
                default:
                    levelInf2Choice1Copy.link = levelInf3;
                    comp.interactionSteps = 1003;
                    break;
            }
        };
        // 无限选项2-2复制
        var levelInf2Choice2Copy = new DiaOption(dialog.LevelChoiceQuit)
        {
            action = delegate
            {
                comp.interactionSteps = Rand.Value switch
                {
                    > 0.9f => 1002,
                    > 0.45f => 1001,
                    _ => 1003
                };
            },
            resolveTree = true,
        };
        // 无限选项3-1
        var levelInf3Choice1 = new DiaOption(dialog.LevelInfChoice1);
        levelInf3Choice1.action = delegate
        {
            switch (Rand.Value)
            {
                case > 0.9f:
                    levelInf3Choice1.link = levelInf3Copy;
                    comp.interactionSteps = 1003;
                    break;
                case > 0.45f:
                    levelInf3Choice1.link = levelInf1;
                    comp.interactionSteps = 1001;
                    break;
                default:
                    levelInf3Choice1.link = levelInf2;
                    comp.interactionSteps = 1002;
                    break;
            }
        };
        // 无限选项3-2
        var levelInf3Choice2 = new DiaOption(dialog.LevelChoiceQuit)
        {
            action = delegate
            {
                comp.interactionSteps = Rand.Value switch
                {
                    > 0.9f => 1003,
                    > 0.45f => 1001,
                    _ => 1002
                };
            },
            resolveTree = true,
        };
        // 无限选项3-1复制
        var levelInf3Choice1Copy = new DiaOption(dialog.LevelInfChoice1);
        levelInf3Choice1Copy.action = delegate
        {
            switch (Rand.Value)
            {
                case > 0.9f:
                    levelInf3Choice1Copy.link = levelInf3;
                    comp.interactionSteps = 1003;
                    break;
                case > 0.45f:
                    levelInf3Choice1Copy.link = levelInf1;
                    comp.interactionSteps = 1001;
                    break;
                default:
                    levelInf3Choice1Copy.link = levelInf2;
                    comp.interactionSteps = 1002;
                    break;
            }
        };
        // 无限选项3-2复制
        var levelInf3Choice2Copy = new DiaOption(dialog.LevelChoiceQuit)
        {
            action = delegate
            {
                comp.interactionSteps = Rand.Value switch
                {
                    > 0.9f => 1003,
                    > 0.45f => 1001,
                    _ => 1002
                };
            },
            resolveTree = true,
        };
        
        // ===================================
        // ====== Infinite Connections =======
        // ===================================
        
        levelInf1.options.Add(levelInf1Choice1);
        if (comp.isTale) levelInf1.options.Add(levelInfTale);
        levelInf1.options.Add(levelInf1Choice2);
        
        levelInf1Copy.options.Add(levelInf1Choice1Copy);
        if (comp.isTale) levelInf1Copy.options.Add(levelInfTale);
        levelInf1Copy.options.Add(levelInf1Choice2Copy);

        levelInf2.options.Add(levelInf2Choice1);
        if (comp.isTale) levelInf2.options.Add(levelInfTale);
        levelInf2.options.Add(levelInf2Choice2);

        levelInf2Copy.options.Add(levelInf2Choice1Copy);
        if (comp.isTale) levelInf2Copy.options.Add(levelInfTale);
        levelInf2Copy.options.Add(levelInf2Choice2Copy);

        levelInf3.options.Add(levelInf3Choice1);
        if (comp.isTale) levelInf3.options.Add(levelInfTale);
        levelInf3.options.Add(levelInf3Choice2);

        levelInf3Copy.options.Add(levelInf3Choice1Copy);
        if (comp.isTale) levelInf3Copy.options.Add(levelInfTale);
        levelInf3Copy.options.Add(levelInf3Choice2Copy);

        // ===================================
        // ========= Finite Layers ===========
        // ===================================
        
        //---------------------------------------------
        // 对话层 11:
        // 雪: 完成任务分支；
        //    O1: 继续对话 -> 对话层: 1001, 进入循环分支
        //    O2: 结束对话 -> 后续进入循环分支
        //---------------------------------------------
        var level11Choice1 = new DiaOption(dialog.Level11Choice1)
        {
            action = delegate {
                comp.interactionSteps = 1001;
                comp.isInfLevel = true;
            },
            link = levelInf1,
        };
        var level11Choice2 = new DiaOption(dialog.LevelChoiceQuit)
        {
            action = delegate {
                comp.interactionSteps = 1001;
                comp.isInfLevel = true;
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
        var level10Choice1 = new DiaOption(dialog.Level10Choice1)
        {
            action = delegate
            {
                YukiVisitorUtils.AddResearchPointsSafe(visitor, Rand.Range(800,1200));
                comp.interactionSteps = 11;
            },
            link = level11,
        };
        var level10Choice2 = new DiaOption(dialog.Level10Choice2)
        {
            action = delegate
            {
                YukiVisitorUtils.SpawnRandomRewards(visitor, Rand.Range(800,1200));
                comp.interactionSteps = 11;
            },
            link = level11,
        };
        var level10Choice3 = new DiaOption(dialog.Level10Choice3)
        {
            action = delegate
            {
                comp.interactionSteps = 5;
                comp.isTale = true;
            },
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
        var level6Choice1 = new DiaOption(dialog.Level6Choice1){resolveTree = true,};
        level6Choice1.action = delegate {
            if (comp.isInfLevel)
            {
                level6Choice1.resolveTree = true;
            }
            else if (!comp.isCasted)
            {
                comp.interactionSteps = 11;
                comp.isWorking = true;
                level6Choice1.resolveTree = true;
                YukiVisitorCast(comp);
            }
            else
            {
                level6Choice1.link = level11;
                comp.interactionSteps = 11;
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
        var level5Choice1Copy = new DiaOption(dialog.Level5Choice1);
        level5Choice1Copy.action = delegate {
            comp.interactionSteps = comp.isInfLevel ? comp.interactionSteps : 5;
            level5.text = Rand.Value > 0.80f ? GrammarUtils.GenerateStory(visitor, negotiator) : GrammarUtils.GenerateTale(visitor, negotiator);
            level5Choice1Copy.link = Rand.Value > 0.05f ? level5 : level6;
        };
        var level5Choice2Copy = new DiaOption(dialog.Level5Choice2);
        level5Choice2Copy.action = delegate {
            if (comp.isInfLevel)
            {
                level5Choice2Copy.resolveTree = true;
            }
            else if (!comp.isCasted)
            {
                level5Choice2Copy.resolveTree = true;
                comp.interactionSteps = 11;
                comp.isWorking = true;
                YukiVisitorCast(comp);

            }
            else
            {
                level5Choice2Copy.link = level11;
                comp.interactionSteps = 11;
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
        var level5Choice1 = new DiaOption(dialog.Level5Choice1);
        level5Choice1.action = delegate {
            comp.interactionSteps = comp.isInfLevel ? comp.interactionSteps : 5;
            level5Copy.text = Rand.Value > 0.80f ? GrammarUtils.GenerateStory(visitor, negotiator) : GrammarUtils.GenerateTale(visitor, negotiator);
            level5Choice1.link = Rand.Value > 0.05f ? level5Copy : level6;
        };
        var level5Choice2 = new DiaOption(dialog.Level5Choice2)
        {
            resolveTree = true
        };
        level5Choice2.action = delegate {
            if (comp.isInfLevel)
            {
                level5Choice2.resolveTree = true;
            }
            else if (!comp.isCasted)
            {
                level5Choice2.resolveTree = true;
                comp.interactionSteps = 11;
                comp.isWorking = true;
                YukiVisitorCast(comp);
            }
            else
            {
                level5Choice2.link = level11;
                comp.interactionSteps = 11;
            }
        };
        level5.options.Add(level5Choice1);
        level5.options.Add(level5Choice2);
        //---------------------------------------------
        // 对话层 4:
        // 雪: 拒绝分支；
        //    O1: 确认完成 -> 对话层: 0, 结束分支
        //---------------------------------------------
        var level4Choice1 = new DiaOption(dialog.Level4Choice1)
        {
            action = delegate
            {
                YukiVisitorCastHidden(comp);
                comp.interactionSteps = 0;
            },
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
        var level3Choice1 = new DiaOption(dialog.Level3Choice1)
        {
            action = delegate
            {
                YukiVisitorUtils.AddResearchPointsSafe(visitor, Rand.Range(350,650));
                YukiVisitorCast(comp);
                comp.interactionSteps = 11;
                comp.isWorking = true;
            },
            resolveTree = true,
        };
        var level3Choice2 = new DiaOption(dialog.Level3Choice2)
        {
            action = delegate
            {
                YukiVisitorUtils.SpawnRandomRewards(visitor, Rand.Range(350,650));
                YukiVisitorCast(comp);
                comp.interactionSteps = 11;
                comp.isWorking = true;
            },
            resolveTree = true,
        };
        var level3Choice3 = new DiaOption(dialog.Level3Choice3)
        {
            action = delegate
            {
                comp.interactionSteps = 5;
                comp.isTale = true;
            },
            link = level5,
        };
        var level3Choice4 = new DiaOption(dialog.Level3Choice4)
        {
            action = delegate { comp.interactionSteps = 4; },
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
        var level2Choice1 = new DiaOption(dialog.Level2Choice1)
        {
            action = delegate
            {
                comp.interactionSteps = 3;
            },
            link = level3,
        };
        var level2Choice2 = new DiaOption(dialog.Level2Choice2)
        {
            action = delegate
            {
                comp.interactionSteps = 4;
            },
            link = level4,
        };
        var level2Choice3 = new DiaOption(dialog.LevelChoiceQuit)
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
        var level1Choice1 = new DiaOption(dialog.Level1Choice1)
        {
            action = delegate
            {
                YukiVisitorCast(comp);
                comp.interactionSteps = 10;
                comp.isWorking = true;
            },
            resolveTree = true,
        };
        var level1Choice2 = new DiaOption(dialog.Level1Choice2)
        {
            action = delegate
            {
                comp.interactionSteps = 2;
            },
            link = level2,
        };
        var level1Choice3 = new DiaOption(dialog.LevelChoiceQuit)
        {
            action = null,
            resolveTree = true,
        };
        level1.options.Add(level1Choice1);
        level1.options.Add(level1Choice2);
        level1.options.Add(level1Choice3);
        
        // ===================================
        // ========== Return Root ============
        // ===================================
        
        if (comp.interactionSteps == 1)
        {
            return level1;
        }
        else if (comp.interactionSteps == 2)
        {
            return level2;
        }
        else if (comp.interactionSteps == 3)
        {
            return level3;
        }
        else if (comp.interactionSteps == 4)
        {
            return level4;
        }
        else if (comp.interactionSteps == 5)
        {
            return level5;
        }
        else if (comp.interactionSteps == 6)
        {
            return level6;
        }
        else if (comp.interactionSteps == 10)
        {
            return level10;
        }
        else if (comp.interactionSteps == 11)
        {
            return level11;
        }
        else if (comp.interactionSteps == 1001)
        {
            return levelInf1;
        }
        else if (comp.interactionSteps == 1002)
        {
            return levelInf2;
        }
        else if (comp.interactionSteps == 1003)
        {
            return levelInf3;
        }
        else
        {
            return levelInf1;
        }
    }
    
    private static void YukiVisitorCast(Comp_YukiVisitor comp)
    {
        if (comp.parent is not Pawn p)
        {
            Log.Error("[Manosaba] comp.parent is not Pawn (YukiVisitorDialogUtils)");
            return;
        }
        if (p.Map == null || !p.Spawned)
        {
            Log.Error("[Manosaba] Pawn.Map is null or Pawn is not spawned (YukiVisitorDialogUtils)");
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
        var job = JobMaker.MakeJob(ModDefOf.UmJobYukiVisitorCast, targetCell);
        job.playerForced = true;
        p.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        comp.isWorking = true; 
    }
    
    private static void YukiVisitorCastHidden(Comp_YukiVisitor comp)
    {
        if (comp.parent is not Pawn p)
        {
            Log.Error("[Manosaba] comp.parent is not Pawn (YukiVisitorDialogUtils)");
            return;
        }
        if (p.Map == null || !p.Spawned)
        {
            Log.Error("[Manosaba] Pawn.Map is null or Pawn is not spawned (Comps.Comp_YukiVisitor.YukiVisitorCastHidden)");
            return;
        }
        comp.interactionSteps = 0;
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
        var job = JobMaker.MakeJob(ModDefOf.UmJobYukiVisitorCastHidden, edgeDest, centerDest);
        job.playerForced = true;
        p.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        comp.isWorking = true;
    }
}