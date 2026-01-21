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
        public Pawn pawn;
        public float sumAllRelation;
        public float normalizedRelation;
        public float boltzmannWeight;
    }
    // 获取所有候选人 (当前地图的自由殖民者), 或其他过滤条件
    // List<Pawn> candidates = map.mapPawns.FreeColonistsSpawned.ToList();
    private static List<Pawn> BoltzmannSelector(List<Pawn> candidates, int count, float invTemp = 5f)
    {
        // 检查输入
        if (candidates == null || candidates.Count == 0)
        {
            if (ManosabaMod.Settings.debugMode) Log.Error("[Manosaba] Cannot select Pawn, input List<Pawn> is null or empty (Utils.RandomSelector.BoltzmannSelector)");
            return new List<Pawn>();
        }
        if ( count <= 0 || invTemp <= 0f)
        {
            if (ManosabaMod.Settings.debugMode) Log.Error("[Manosaba] Cannot select Pawn, input parameters are invalid (Utils.RandomSelector.BoltzmannSelector)");
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
                pawn = pawn, 
                sumAllRelation = sum 
            });
        }
        if (dataList.Count > 0)
        {
            var minSum = dataList.Min(d => d.sumAllRelation);
            var maxSum = dataList.Max(d => d.sumAllRelation);
            var range = maxSum - minSum;

            foreach (var data in dataList)
            {
                if (range <= float.Epsilon)
                {
                    data.normalizedRelation = 0f;
                }
                else
                {
                    data.normalizedRelation = (data.sumAllRelation - minSum) / range * invTemp;
                }
                data.boltzmannWeight = Mathf.Exp(-data.normalizedRelation);
            }
        }
        var selectedPawns = new List<Pawn>();
        for (var i = 0; i < count; i++)
        {
            if (dataList.TryRandomElementByWeight(d => d.boltzmannWeight, out var selected))
            {
                selectedPawns.Add(selected.pawn);
                dataList.Remove(selected);
            }
            else
            {
                if (ManosabaMod.Settings.debugMode) Log.Error("[Manosaba] Cannot select Pawn, as all weights are zero (Utils.RandomSelector.BoltzmannSelector)");
                break;
            }
        }
            
        if (ManosabaMod.Settings.debugMode)
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
            if (ManosabaMod.Settings.debugMode) Log.Error("[Manosaba] Invalid input parameter, random selection failed (Utils.RandomSelector.TryAddDummyToRandomPawnOnMap)");
            return;
        }
        if (map == null)
        {
            if (ManosabaMod.Settings.debugMode) Log.Warning("[Manosaba] Invalid map, random selection failed (Utils.RandomSelector.TryAddDummyToRandomPawnOnMap)");
            return;
        }
        var candidates = map.mapPawns.FreeColonistsSpawned
            .Where(p => 
                !p.health.hediffSet.HasHediff(ModDefOf.UmHediffHumanDummy)&&
                !p.health.hediffSet.HasHediff(ModDefOf.UmHediffYukiDummy))
            .ToList();

        switch (candidates.Count)
        {
            case 0 when ManosabaMod.Settings.debugMode:
                Log.Message($"[Manosaba] No qualified colonists on the current map (Utils.RandomSelector.TryAddDummyToRandomPawnOnMap)");
                break;
            case > 0:
            {
                if (ManosabaMod.Settings.debugMode) Log.Message($"[Manosaba] Try to select {selectCount} colonists from {candidates.Count} candidates (Utils.RandomSelector.TryAddDummyToRandomPawnOnMap)");
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