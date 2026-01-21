using HarmonyLib;
using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Patches;

// 修改月代雪二级恋爱系数：忽略种族、年龄问题
[HarmonyPatch(typeof(Pawn_RelationsTracker), "SecondaryLovinChanceFactor")]
public static class Patch_SecondaryLovinChanceFactor_Prefix
{
    private static readonly AccessTools.FieldRef<Pawn_RelationsTracker, Pawn> PawnFieldRef = 
        AccessTools.FieldRefAccess<Pawn_RelationsTracker, Pawn>("pawn");
        
    public static bool Prefix(Pawn_RelationsTracker __instance, Pawn otherPawn, ref float __result)
    {
        var pawn = PawnFieldRef(__instance);
        var thisIsYuki = pawn.kindDef == ModDefOf.UmPawnKindYukiColonist ||
                         pawn.kindDef == ModDefOf.UmPawnKindYukiVisitor;
        var otherIsYuki = otherPawn.kindDef == ModDefOf.UmPawnKindYukiColonist ||
                          otherPawn.kindDef == ModDefOf.UmPawnKindYukiVisitor;
        if (!thisIsYuki && !otherIsYuki) return true;
        __result = SecondaryLovinChanceFactor(pawn, otherPawn);
        return false;
    }

    private static float SecondaryLovinChanceFactor(Pawn pawn, Pawn otherPawn)
    {
        if (pawn == otherPawn) return 0f;
        if (!pawn.RaceProps.Humanlike || !otherPawn.RaceProps.Humanlike) return 0f;
        //if (pawn.def != otherPawn.def) return 0f;
        if (pawn.story is not { traits: not null }) return PrettinessFactor(otherPawn);
        if (pawn.story.traits.HasTrait(TraitDefOf.Asexual)) return 0f;
        if (pawn.story.traits.HasTrait(TraitDefOf.Bisexual)) return PrettinessFactor(otherPawn);
        if (pawn.story.traits.HasTrait(TraitDefOf.Gay) && otherPawn.gender != pawn.gender) return 0f;
        return otherPawn.gender == pawn.gender ? 0f : PrettinessFactor(otherPawn);
    }
    private static float PrettinessFactor(Pawn otherPawn)
    {
        var num = 0f;
        if (otherPawn.RaceProps.Humanlike) num = otherPawn.GetStatValue(StatDefOf.PawnBeauty);
        return num switch
        {
            < 0f => 0.3f,
            > 0f => 2.3f,
            _ => 1f
        };
    }
}