using HarmonyLib;
using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Patches;

// 修改月代雪可以恋爱：单人筛选
[HarmonyPatch(typeof(RelationsUtility))]
[HarmonyPatch("RomanceEligible")]
public static class Patch_RomanceEligible_Prefix
{
    public static bool Prefix(Pawn pawn, bool initiator, bool forOpinionExplanation, ref AcceptanceReport __result)
    {
        var isYuki = pawn.kindDef == ModDefOf.UmPawnKindYukiColonist ||
                     pawn.kindDef == ModDefOf.UmPawnKindYukiVisitor;
        if (!isYuki) return true;
        __result = RomanceEligible(pawn, initiator, forOpinionExplanation );
        return false;
    }
        
    public static AcceptanceReport RomanceEligible(Pawn pawn, bool initiator, bool forOpinionExplanation)
    {
        var state = !initiator || forOpinionExplanation;
        if (pawn.IsPrisoner)
        {
            return state ? AcceptanceReport.WasRejected : "CantRomanceInitiateMessagePrisoner".Translate(pawn).CapitalizeFirst();
        }
        if (!forOpinionExplanation && pawn.Downed)
        {
            return initiator ? "CantRomanceInitiateMessageDowned".Translate(pawn).CapitalizeFirst() : "CantRomanceTargetDowned".Translate();
        }
        if (initiator && pawn.IsSlave)
        {
            return !forOpinionExplanation ? "CantRomanceInitiateMessageSlave".Translate(pawn).CapitalizeFirst() : AcceptanceReport.WasRejected;
        }
            
            
        var story = pawn.story;
        if (story != null && story.traits?.HasTrait(TraitDefOf.Asexual) == true)
        {
            return state ? AcceptanceReport.WasRejected : "CantRomanceInitiateMessageAsexual".Translate(pawn).CapitalizeFirst();
        }
        if (initiator && !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
        {
            return !forOpinionExplanation ? "CantRomanceInitiateMessageTalk".Translate(pawn).CapitalizeFirst() : AcceptanceReport.WasRejected;
        }
        if (!forOpinionExplanation && pawn.Drafted)
        {
            return initiator ? "CantRomanceInitiateMessageDrafted".Translate(pawn).CapitalizeFirst() : "CantRomanceTargetDrafted".Translate();
        }

        if (pawn.MentalState != null)
        {
            return !state ? "CantRomanceInitiateMessageMentalState".Translate(pawn).CapitalizeFirst() : "CantRomanceTargetMentalState".Translate();
        }
        return true;
    }
}