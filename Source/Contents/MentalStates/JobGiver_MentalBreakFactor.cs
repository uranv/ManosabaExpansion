using RimWorld;
using Verse;
using Verse.AI;

namespace UranvManosaba.Contents.MentalStates;

public class JobGiver_MentalBreakFactor : ThinkNode_JobGiver
{
    protected override Job TryGiveJob(Pawn pawn)
    {
        // 调用下面的静态方法获取目标
        var target = FindHatedTarget(pawn);
            
        if (target == null) return null;
            
        var job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
        job.maxNumMeleeAttacks = 1;
        job.expiryInterval = 120;
        job.checkOverrideOnExpire = true;
        return job;
    }

    //  Worker 和 MentalState 外部调用
    public static Pawn FindHatedTarget(Pawn pawn)
    {
        if (pawn.Map == null) return null;

        // 获取地图上所有可见的、属于玩家派系的、活着的 Pawn (排除自己)
        var potentialTargets = pawn.Map.mapPawns.FreeColonistsAndPrisonersSpawned
            .Where(p => p != pawn && !p.Dead && pawn.CanReach(p, PathEndMode.Touch, Danger.Deadly))  // 不排除 !p.Downed, 致死崩溃
            .ToList();

        if (potentialTargets.Count == 0) return null;

        Pawn bestTarget = null;
        var lowestOpinion = 1000;

        foreach (var target in potentialTargets)
        {
            if (pawn.relations == null) continue;
                
            var opinion = pawn.relations.OpinionOf(target);
                
            if (opinion < lowestOpinion)
            {
                lowestOpinion = opinion;
                bestTarget = target;
            }
        }

        return bestTarget;
    }
}