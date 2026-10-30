using HarmonyLib;
using RimWorld;
using UnityEngine;
using UranvManosaba.Contents.Utils;
using Verse;
using Verse.AI;

namespace UranvManosaba.Contents.Patches;

// 任一方为月代雪时，双方均豁免恋爱年龄限制。
[HarmonyPatch(typeof(RelationsUtility))]
[HarmonyPatch("RomanceEligiblePair")]
public static class Patch_RomanceEligiblePair_Prefix
{
    public static bool Prefix(Pawn initiator, Pawn target, bool forOpinionExplanation, ref AcceptanceReport __result)
    {
        var initYuki = YukiGeneralUtils.IsYuki(initiator);
        var tarYuki = YukiGeneralUtils.IsYuki(target);
        if (!initYuki && !tarYuki) return true;
        __result = CustomRomanceEligiblePair(initiator, target, forOpinionExplanation);
        return false;
    }

    private static AcceptanceReport CustomRomanceEligiblePair(Pawn initiator, Pawn target, bool forOpinionExplanation)
    {
        // 检查目标
        if (initiator == target)
        {
            return false;
        }
        // 检查冷却
        if (initiator.relations.IsTryRomanceOnCooldown)
        {
            return "RomanceOnCooldown".Translate();
        }
        // 检查取向
        if (!RelationsUtility.AttractedToGender(initiator, target.gender) || !RelationsUtility.AttractedToGender(target, initiator.gender))
        {
            return !forOpinionExplanation ? AcceptanceReport.WasRejected : "CantRomanceTargetSexuality".Translate();
        }
        // 检查哺乳
        if (ChildcareUtility.CanSuckle(target, out _))
        {
            return false;
        }
        // 现有关系检查
        var directPawnRelation = LovePartnerRelationUtility.ExistingLoveRealtionshipBetween(initiator, target, allowDead: false);
        if (directPawnRelation != null)
        {
            var genderSpecificLabel = directPawnRelation.def.GetGenderSpecificLabel(target);
            return "RomanceChanceExistingRelation".Translate(initiator.Named("PAWN"), genderSpecificLabel.Named("RELATION"));
        }
        // 发起方资格检查：与目标方一样只豁免年龄，保留其他限制。
        if (!Patch_RomanceEligible_Prefix.RomanceEligible(initiator, initiator: true, forOpinionExplanation))
        {
            return false;
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
        // 目标方资格检查：配对中已有月代雪，因此目标也豁免年龄。
        var targetReport = Patch_RomanceEligible_Prefix.RomanceEligible(target, initiator: false, forOpinionExplanation);
        if (!targetReport)
        {
            return targetReport;
        }
        // 可达性检查
        if ((!forOpinionExplanation && !initiator.CanReach(target, PathEndMode.Touch, Danger.Deadly)) ||
            target.IsForbidden(initiator))
        {
            return "CantRomanceTargetUnreachable".Translate();
        }
        // 成功率检查
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