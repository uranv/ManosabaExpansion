using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace UranvManosaba.Contents.Patches;

// 修改月代雪可以恋爱：pair筛选
[HarmonyPatch(typeof(RelationsUtility))]
[HarmonyPatch("RomanceEligiblePair")]
public static class Patch_RomanceEligiblePair_Prefix
{
    public static bool Prefix(Pawn initiator, Pawn target, bool forOpinionExplanation, ref AcceptanceReport __result)
    {
        var initYuki = initiator.kindDef == ModDefOf.UmPawnKindYukiColonist ||
                       initiator.kindDef == ModDefOf.UmPawnKindYukiVisitor;
        var tarYuki = target.kindDef == ModDefOf.UmPawnKindYukiColonist ||
                      target.kindDef == ModDefOf.UmPawnKindYukiVisitor;
        if (!initYuki && !tarYuki) return true;
        __result = CustomRomanceEligiblePair(initiator, target, forOpinionExplanation);
        return false;
    }

    private static AcceptanceReport CustomRomanceEligiblePair(Pawn initiator, Pawn target, bool forOpinionExplanation)
    {
        // 检查目标
        if (initiator == target) return false;
        // 检查冷却
        if (initiator.relations.IsTryRomanceOnCooldown) return "RomanceOnCooldown".Translate();
        // 检查取向
        if (!RelationsUtility.AttractedToGender(initiator, target.gender) || !RelationsUtility.AttractedToGender(target, initiator.gender))
        {
            return !forOpinionExplanation ? AcceptanceReport.WasRejected : "CantRomanceTargetSexuality".Translate();
        }
        // 检查哺乳
        if (ChildcareUtility.CanSuckle(target, out _)) return false;
        // 现有关系检查
        var directPawnRelation = LovePartnerRelationUtility.ExistingLoveRealtionshipBetween(initiator, target, allowDead: false);
        if (directPawnRelation != null)
        {
            var genderSpecificLabel = directPawnRelation.def.GetGenderSpecificLabel(target);
            return "RomanceChanceExistingRelation".Translate(initiator.Named("PAWN"), genderSpecificLabel.Named("RELATION"));
        }
        // 乱伦检查
        if (IsIncestuous(initiator, target))
        {
            return !forOpinionExplanation ? AcceptanceReport.WasRejected : "CantRomanceTargetIncest".Translate();
        }
        // 囚犯检查
        if (target.IsPrisoner)
        {
            return !forOpinionExplanation ? AcceptanceReport.WasRejected : "CantRomanceTargetPrisoner".Translate();
        }
        // 好感度检查
        if (target.relations.OpinionOf(initiator) <= 5)
        {
            return "CantRomanceTargetOpinion".Translate();
        }
        // 单人资格检查
        var targetReport = Patch_RomanceEligible_Prefix.RomanceEligible(target, initiator: false, forOpinionExplanation);
        if (!targetReport)
        {
            return targetReport;
        }
            
        // 【昂贵检查区】

        // 可达性检查 (寻路，较慢)
        if ((!forOpinionExplanation && !initiator.CanReach(target, PathEndMode.Touch, Danger.Deadly)) ||
            target.IsForbidden(initiator))
        {
            return "CantRomanceTargetUnreachable".Translate();
        }
        // 成功率检查 (非常慢，涉及大量 Def 遍历和计算)
        if (!forOpinionExplanation && InteractionWorker_RomanceAttempt.SuccessChance(initiator, target, 1f) <= 0f)
        {
            return "CantRomanceTargetZeroChance".Translate();
        }

        return true;
    }
        
    private static bool IsIncestuous(Pawn one, Pawn two)
    {
        foreach (var relation in one.GetRelations(two))
        {
            if (!Mathf.Approximately(relation.romanceChanceFactor, 1f))
            {
                return true;
            }
        }
        return false;
    }
}