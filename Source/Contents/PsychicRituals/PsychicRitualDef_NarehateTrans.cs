using RimWorld;
using Verse;
using Verse.AI.Group;

namespace UranvManosaba.Contents.PsychicRituals;

public class PsychicRitualDef_NarehateTrans : PsychicRitualDef_InvocationCircle
{
    public SimpleCurve SeverityFromQualityCurve;

    public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
    {
        List<PsychicRitualToil> list = base.CreateToils(psychicRitual, graph);
        list.Add(new PsychicRitualToil_NarehateTrans(TargetRole, SeverityFromQualityCurve));
        return list;
    }
    // 结果描述预览
    public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
    {
        float minSev = SeverityFromQualityCurve.Evaluate(qualityRange.min);
        float maxSev = SeverityFromQualityCurve.Evaluate(qualityRange.max);
        return outcomeDescription.Formatted(minSev.ToString("0.##"), maxSev.ToString("0.##"));
    }
    // 重写候选人池：添加魔女残骸 mutant
    public override PsychicRitualCandidatePool FindCandidatePool()
    {
        var candidates = new List<Pawn>(Find.CurrentMap.mapPawns.FreeColonistsAndPrisonersSpawned.Where(p => !p.IsSubhuman || IsNarehateMutant(p)));
        foreach (var p in Find.CurrentMap.mapPawns.AllPawnsSpawned)
        {
            if (candidates.Contains(p)) continue;
            if (IsNarehateMutant(p)) candidates.Add(p);
        }
        return new PsychicRitualCandidatePool(candidates, new List<Pawn>());
        bool IsNarehateMutant(Pawn p) => p.IsMutant && p.mutant.Def.defName == "UmMutantNarehate";
    }
    // 进一步筛选仪式目标在 Patch_RitualPawnCanDo.cs 中实现
}