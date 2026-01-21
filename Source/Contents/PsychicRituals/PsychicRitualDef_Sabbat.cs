using RimWorld;
using Verse;
using Verse.AI.Group;

namespace UranvManosaba.Contents.PsychicRituals;

public class PsychicRitualDef_Sabbat : PsychicRitualDef_InvocationCircle
{
    private static Dictionary<PsychicRitualRoleDef, List<IntVec3>> _tmpParticipants =
        new Dictionary<PsychicRitualRoleDef, List<IntVec3>>(8);

    // 预览质量
    public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber,
        PsychicRitualRoleAssignments assignments)
    {
        float minSev = qualityRange.min;
        int qualityOffset = -2 +
                            (minSev >= ManosabaMod.Settings.thresholdMinusOne / 100f ? 1 : 0) +
                            (minSev >= ManosabaMod.Settings.thresholdZero / 100f ? 1 : 0) +
                            (minSev >= ManosabaMod.Settings.thresholdPlusOne / 100f ? 1 : 0);
        return outcomeDescription.Formatted($"{(qualityOffset > 0 ? "+" : "")}{qualityOffset}");
    }

    // 手动构建部分原版 Toils
    public override List<PsychicRitualToil> CreateToils(Verse.AI.Group.PsychicRitual psychicRitual, PsychicRitualGraph parent)
    {
        // pawn 站位信息
        var rolePositions = GenerateRolePositions_Copy(psychicRitual.assignments);

        var list = new List<PsychicRitualToil>();
        // （原版）仪式集合
        list.Add(new PsychicRitualToil_GatherForInvocation(psychicRitual, this, rolePositions));
        // （修改）仪式引导和销毁祭品
        var invokeToil = new PsychicRitualToil_Sabbat_InvokeHorax(
            InvokerRole, rolePositions.TryGetValue(InvokerRole),
            TargetRole, rolePositions.TryGetValue(TargetRole),
            ChanterRole, rolePositions.TryGetValue(ChanterRole),
            DefenderRole, rolePositions.TryGetValue(DefenderRole),
            RequiredOffering)
        {
            hoursUntilHoraxEffect = hoursUntilHoraxEffect.RandomInRange,
            hoursUntilOutcome = hoursUntilOutcome.RandomInRange
        };
        list.Add(invokeToil);
        var outcomeToil = new PsychicRitualToil_Sabbat_Outcome(
            InvokerRole, rolePositions.TryGetValue(InvokerRole),
            ChanterRole, rolePositions.TryGetValue(ChanterRole));
        list.Add(outcomeToil);
        list.Add(new PsychicRitualToil_Wait(120));
        return list;
    }
        
    // 禁止原因：已经存在小雪
    public override IEnumerable<string> BlockingIssues(PsychicRitualRoleAssignments assignments, Map map)
    {
        foreach (var item in base.BlockingIssues(assignments, map))
        {
            yield return item;
        }

        var yukiColonistKind = ModDefOf.UmPawnKindYukiColonist;
        var yukiVisitorKind = ModDefOf.UmPawnKindYukiVisitor;
        if (yukiColonistKind == null || yukiVisitorKind == null) yield break;
        var exists = map.mapPawns.AllPawnsSpawned.Any(p =>
            (p.kindDef == yukiColonistKind || p.kindDef == yukiVisitorKind) && !p.Dead);
        if (exists)
            yield return "Manosaba_SabbatReason_YukiExists".Translate(ManosabaMod.YukiNameDef.Named("YUKI"));
    }
        
    // 复制原版私有方法 RimWorld.PsychicRitualDef_InvocationCircle.GenerateRolePositions
    private IReadOnlyDictionary<PsychicRitualRoleDef, List<IntVec3>> GenerateRolePositions_Copy(PsychicRitualRoleAssignments assignments)
    {
        _tmpParticipants.ClearAndPoolValueLists();
        foreach (var role in Roles)
        {
            _tmpParticipants[role] = SimplePool<List<IntVec3>>.Get();
        }
        var targetCell = assignments.Target.Cell;
        var map = assignments.Target.Map;
        // 角色计数
        int invokerCount = InvokerRole == null ? 0 : assignments.RoleAssignedCount(InvokerRole);
        int chanterCount = ChanterRole == null ? 0 : assignments.RoleAssignedCount(ChanterRole);
        int totalInnerCount = invokerCount + chanterCount;
        // 用于计算极坐标角度的全局索引
        int currentIdx = 0;
        // Invoker 坐标
        if (InvokerRole != null && invokerCount > 0)
        {
            for (int i = 0; i < invokerCount; i++)
            {
                int retry = 0;
                IntVec3 cell;
                do
                {
                    cell = targetCell + IntVec3.FromPolar(360f * currentIdx++ / totalInnerCount,
                        invocationCircleRadius);
                } while (!cell.Walkable(map) && retry++ <= 10);

                if (retry >= 10) cell = targetCell;
                _tmpParticipants[InvokerRole].Add(cell);
            }
        }
        // Chanter 坐标
        if (ChanterRole != null && invokerCount > 0)
        {
            for (int i = 0; i < chanterCount; i++)
            {
                var cell = targetCell +
                           IntVec3.FromPolar(360f * currentIdx++ / totalInnerCount, invocationCircleRadius);
                _tmpParticipants[ChanterRole].Add(cell);
            }
        }
        // Target 坐标
        if (TargetRole != null)
        {
            int targetRoleCount = assignments.RoleAssignedCount(TargetRole);
            for (int i = 0; i < targetRoleCount; i++)
            {
                _tmpParticipants[TargetRole].Add(targetCell);
            }
        }

        // Defender 坐标
        if (DefenderRole == null) return _tmpParticipants;
        int defenderCount = assignments.RoleAssignedCount(DefenderRole);
        if (defenderCount <= 0) return _tmpParticipants;
        bool playerRitual = assignments.AllAssignedPawns.Any(x => x.Faction == Faction.OfPlayer);
        for (int i = 0; i < defenderCount; i++)
        {
            var cell = targetCell + IntVec3.FromPolar(360f * i / defenderCount, invocationCircleRadius + 5f);
            cell = GetBestStandableRolePosition(playerRitual, cell, targetCell, map);
            _tmpParticipants[DefenderRole].Add(cell);
        }
        return _tmpParticipants;
    }
        
}