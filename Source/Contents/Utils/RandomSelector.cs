using RimWorld;
using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Utils;
/// <summary>
/// Random Selector using Boltzmann distribution
/// Normalized Sum of Pawn Relations -> Energy
/// ModSettings -> inverseTemperature
/// </summary>
public static class RandomSelector
{
    // =============================
    // ---- 玻尔兹曼分布随机选择器 ----
    // =============================
    // 根据殖民者之间的关系选择指定数量的 Pawn
    // 总计关系越好（Sum_relation 越高）的 Pawn 被选择的概率越低
    // 使用 Boltzmann 分布进行加权随机选择
    private class PawnWeightData
    {
        public Pawn Candidate;
        public float SumAllRelation;
        public float NormalizedRelation;
        public float BoltzmannWeight;
    }
    // 获取所有候选人 (当前地图的自由殖民者), 或其他过滤条件
    // List<Pawn> candidates = map.mapPawns.FreeColonistsSpawned.ToList();
    private static List<Pawn> BoltzmannSelector(List<Pawn> candidates, int count, float invTemp = 5f)
    {
        // 检查输入
        if (candidates == null || candidates.Count == 0)
        {
            if (ManosabaMod.Settings.DebugMode) Log.Error("[Manosaba] Cannot select Pawn, input List<Pawn> is null or empty (Utils.RandomSelector.BoltzmannSelector)");
            return new List<Pawn>();
        }
        if (count <= 0)
        {
            if (ManosabaMod.Settings.DebugMode) Log.Error("[Manosaba] Cannot select Pawn, input parameters are invalid (Utils.RandomSelector.BoltzmannSelector)");
            return new List<Pawn>();
        }
        // 如果候选人不足 m 人则选择所有人
        if (candidates.Count <= count)
        {
            return candidates;
        }
        // 随机选择
        var dataList = new List<PawnWeightData>();
        foreach (var pawn in candidates)
        {
            var sum = 0f;
            foreach (var other in candidates)
            {
                if (pawn == other) continue;
                sum += pawn.relations.OpinionOf(other);
            }
            dataList.Add(new PawnWeightData
            {
                Candidate = pawn,
                SumAllRelation = sum
            });
        }
        if (dataList.Count > 0)
        {
            var minSum = dataList.Min(d => d.SumAllRelation);
            var maxSum = dataList.Max(d => d.SumAllRelation);
            var range = maxSum - minSum;

            foreach (var data in dataList)
            {
                if (range <= float.Epsilon)
                {
                    data.NormalizedRelation = 0f;
                }
                else
                {
                    data.NormalizedRelation = (data.SumAllRelation - minSum) / range * invTemp;
                }
                data.BoltzmannWeight = Mathf.Exp(-data.NormalizedRelation);
            }
        }
        var selectedPawns = new List<Pawn>();
        for (var i = 0; i < count; i++)
        {
            if (dataList.TryRandomElementByWeight(d => d.BoltzmannWeight, out var selected))
            {
                selectedPawns.Add(selected.Candidate);
                dataList.Remove(selected);
            }
            else
            {
                if (ManosabaMod.Settings.DebugMode) Log.Error("[Manosaba] Cannot select Pawn, as all weights are zero (Utils.RandomSelector.BoltzmannSelector)");
                break;
            }
        }

        if (ManosabaMod.Settings.DebugMode)
        {
            var debugMsg = selectedPawns.Aggregate("[Manosaba] Choose pawns: {", (current, p) => current + $"{p.LabelShort}, ");
            debugMsg += "} from candidates: {";
            debugMsg = candidates.Aggregate(debugMsg, (current, p) => current + $"{p.LabelShort}, ");
            debugMsg += "} (Utils.RandomSelector.BoltzmannSelector)";
            Log.Message(debugMsg);
        }

        return selectedPawns;
    }

    // 概率选择当前地图上合规 Pawn 添加 人类 dummy
    public static void TryAddDummyToRandomPawnOnMap(Map map, int selectCount = 1, float inverseTemperature = 5f)
    {
        if (selectCount <= 0)
        {
            if (ManosabaMod.Settings.DebugMode) Log.Error("[Manosaba] Invalid input parameter, random selection failed (Utils.RandomSelector.TryAddDummyToRandomPawnOnMap)");
            return;
        }
        if (map == null)
        {
            if (ManosabaMod.Settings.DebugMode) Log.Warning("[Manosaba] Invalid map, random selection failed (Utils.RandomSelector.TryAddDummyToRandomPawnOnMap)");
            return;
        }
        var candidates = map.mapPawns.FreeColonistsSpawned
            .Where(p =>
                !p.IsQuestLodger() &&  // 排除任务成员 v1.0.6
                !p.IsSlaveOfColony &&  // 排除奴隶成员 v1.0.6
                !p.IsMutant &&  // 排除变种人(非人)成员 v1.1.0
                !p.health.hediffSet.HasHediff(ModDefOf.UmHediffHumanDummy)&&
                !p.health.hediffSet.HasHediff(ModDefOf.UmHediffMutantDummy)&&
                !p.health.hediffSet.HasHediff(ModDefOf.UmHediffYukiDummy))
            .ToList();

        switch (candidates.Count)
        {
            case 0 when ManosabaMod.Settings.DebugMode:
                Log.Message($"[Manosaba] No qualified colonists on the current map (Utils.RandomSelector.TryAddDummyToRandomPawnOnMap)");
                break;
            case > 0:
            {
                if (ManosabaMod.Settings.DebugMode) Log.Message($"[Manosaba] Try to select {selectCount} colonists from {candidates.Count} candidates (Utils.RandomSelector.TryAddDummyToRandomPawnOnMap)");
                var selected = BoltzmannSelector(candidates, selectCount, inverseTemperature);
                if (selected is { Count: > 0 })
                {
                    foreach (var p in selected)
                    {
                        p.health.AddHediff(ModDefOf.UmHediffHumanDummy);
                    }
                }
                break;
            }
        }
    }
}