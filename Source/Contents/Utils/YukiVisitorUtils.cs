using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UranvManosaba.Contents.Utils;

public static class YukiVisitorUtils
{
    // 奖励：按研究界面显示的进度添加普通研究工作量。
    public static void AddResearchPointsSafe(Pawn pawn, float amount = 1000f)
    {
        if (amount <= 0f) return;
        var manager = Find.ResearchManager;
        var allUnfinished = DefDatabase<ResearchProjectDef>.AllDefsListForReading
            .Where(def => def.knowledgeCategory == null && def.baseCost > 0f && !def.IsFinished && def.CanStartNow)
            .ToList();

        if (allUnfinished.NullOrEmpty())
        {
            return;
        }
        var safeCandidates = allUnfinished
            .Where(def => (def.CostApparent - def.ProgressApparent) > amount)
            .ToList();
        ResearchProjectDef targetProject;
        if (!safeCandidates.NullOrEmpty())
        {
            targetProject = safeCandidates.RandomElement();
        }
        else
        {
            targetProject = allUnfinished
                .OrderByDescending(def => def.CostApparent - def.ProgressApparent)
                .FirstOrDefault();
        }
        if (targetProject == null) return;
        // AddProgress 使用内部进度；按玩家科技系数换算为界面上的奖励点数。
        var progressBefore = targetProject.ProgressApparent;
        manager.AddProgress(targetProject, amount / targetProject.CostFactor(Faction.OfPlayer.def.techLevel), pawn);
        // 完成项目时原版会截断进度，通知使用实际增加的界面点数。
        var addedProgressText = (targetProject.ProgressApparent - progressBefore).ToString("F0");
        var letterLabel = "Manosaba_CompYukiVisitor_ResearchPoints_LetterLabel".Translate(addedProgressText);
        var letterText = "Manosaba_CompYukiVisitor_ResearchPoints_LetterText".Translate(targetProject.LabelCap, addedProgressText);
        Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent);
    }


    // 奖励: 随机物品
    private static List<ThingDef> _cachedPossibleRewards;

    private static List<ThingDef> GetPossibleRewards()
    {
        if (_cachedPossibleRewards == null)
        {
            _cachedPossibleRewards = new List<ThingDef>
            {
                ThingDefOf.Silver,                     // val=1
                ThingDefOf.Steel,                      // val=1.9
                DefDatabase<ThingDef>.GetNamed("Synthread", false), // val=4
                ThingDefOf.Jade,                       // val=5
                DefDatabase<ThingDef>.GetNamed("DevilstrandCloth", false),  // val=5.5
                ThingDefOf.Uranium,                    // val=6
                ThingDefOf.Plasteel,                   // val=9
                DefDatabase<ThingDef>.GetNamed("Hyperweave", false), // val=9
                ThingDefOf.Gold,                       // val=10
                ThingDefOf.ComponentIndustrial,        // val=32
                ThingDefOf.ComponentSpacer,            // val=200
                DefDatabase<ThingDef>.GetNamed("Apparel_PsychicShockLance", false),    // val=550
                DefDatabase<ThingDef>.GetNamed("Apparel_PsychicInsanityLance", false), // val=650
            };
            _cachedPossibleRewards.RemoveAll(x => x == null);
        }
        return _cachedPossibleRewards;
    }

    // 放置奖励
    public static void SpawnRandomRewards(Pawn pawn, float targetValue)
    {
        if (pawn is not { Spawned: true }) return;
        Dictionary<ThingDef, int> rewardsToSpawn = CalculateRewards(targetValue);
        string letterRewards="";
        foreach (var pair in rewardsToSpawn)
        {

            var def = pair.Key;
            if (pair.Value !=0) letterRewards += $"{pair.Key.LabelCap} x{pair.Value}, ";
            int totalCount = pair.Value;
            while (totalCount > 0)
            {
                var thing = ThingMaker.MakeThing(def);
                int stackCount = Mathf.Min(totalCount, def.stackLimit);
                thing.stackCount = stackCount;
                GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                totalCount -= stackCount;
            }
        }
        // 提示信息
        var letterLabel = "Manosaba_CompYukiVisitor_Item_letterLabel".Translate(targetValue);
        var letterText = "Manosaba_CompYukiVisitor_Item_letterText".Translate(targetValue);
        var letterTextAfter = "Manosaba_CompYukiVisitor_Item_letterTextAfter".Translate();
        if (rewardsToSpawn.Count <= 6) letterTextAfter = null;
        Find.LetterStack.ReceiveLetter(letterLabel, letterText + letterRewards + letterTextAfter, LetterDefOf.PositiveEvent);
        // 音效：一般掉落
        SoundDefOf.Standard_Drop.PlayOneShot(pawn);
    }
    // 生成奖励物品集合
    private static Dictionary<ThingDef, int> CalculateRewards(float budget)
    {
        Dictionary<ThingDef, int> result = new Dictionary<ThingDef, int>();
        List<ThingDef> pool = GetPossibleRewards();
        float remainingBudget = budget;

        int attempts = 0;
        while (remainingBudget > 0 && attempts < 1000)
        {
            attempts++;
            var affordableItems = pool.Where(t => t.BaseMarketValue <= remainingBudget + 0.1f).ToList();
            if (!affordableItems.Any()) break;
            affordableItems.TryRandomElementByWeight(d => d.BaseMarketValue, out var chosenDef);//RandomElement();
            float price = chosenDef.BaseMarketValue;
            int maxCanBuy = Mathf.FloorToInt(remainingBudget / price);
            var countToBuy = maxCanBuy == 1 ? 1 : Rand.RangeInclusive(price < 10f ? Mathf.CeilToInt(maxCanBuy * 0.5f) : 1, maxCanBuy);
            if (result.ContainsKey(chosenDef))
            {
                result[chosenDef] += countToBuy;
            }
            else
            {
                result.Add(chosenDef, countToBuy);
            }
            remainingBudget -= countToBuy * price;
        }

        if (remainingBudget >= 1f && result.ContainsKey(ThingDefOf.Silver))
        {
            result[ThingDefOf.Silver] += Mathf.FloorToInt(remainingBudget);
        }
        else if (remainingBudget >= 1f && !result.ContainsKey(ThingDefOf.Silver))
        {
            result.Add(ThingDefOf.Silver, Mathf.FloorToInt(remainingBudget));
        }

        return result;
    }
}