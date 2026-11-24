using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Patches;

[HarmonyPatch(typeof(ApparelUtility), nameof(ApparelUtility.HasPartsToWear))]
public static class Patch_NarehateApparelWear
{
    public static void Postfix(Pawn p, ThingDef apparel, ref bool __result)
    {
        if (apparel == ModDefOf.UmThingApparelNarehateMask)
            __result = p.RaceProps.Humanlike && p.health.hediffSet.HasHead;
        else if (apparel == ModDefOf.UmThingApparelNarehateMantle)
            __result = p.RaceProps.Humanlike;
    }
}

[HarmonyPatch(typeof(ApparelProperties), nameof(ApparelProperties.GetInterferingBodyPartGroups))]
public static class Patch_NarehateApparelSlots
{
    public static void Postfix(ApparelProperties __instance, ref BodyPartGroupDef[] __result)
    {
        // The cosmetic groups have no anatomical coverage, but each still occupies one apparel slot.
        if (__instance.layers.Contains(ModDefOf.UmApparelLayerNarehateOverlay))
            __result = __instance.bodyPartGroups.ToArray();
    }
}

[HarmonyPatch]
public static class Patch_NarehateApparelRendering
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.EnumeratorMoveNext(AccessTools.Method(
            typeof(DynamicPawnRenderNodeSetup_Apparel), nameof(DynamicPawnRenderNodeSetup_Apparel.GetDynamicNodes)));
        yield return AccessTools.Method(typeof(PawnRenderTree), "AdjustParms");
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Use one apparel list for the render nodes and the flags that can hide hair or eyes.
        // The mask hides other headgear. The mantle hides other body apparel.
        return instructions.MethodReplacer(
            AccessTools.PropertyGetter(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.WornApparel)),
            AccessTools.Method(typeof(Patch_NarehateApparelRendering), nameof(VisibleApparel)));
    }

    private static List<Apparel> VisibleApparel(Pawn_ApparelTracker tracker)
    {
        var worn = tracker.WornApparel;
        var mask = worn.Any(apparel => apparel.def == ModDefOf.UmThingApparelNarehateMask);
        var mantle = worn.Any(apparel => apparel.def == ModDefOf.UmThingApparelNarehateMantle);
        if (!mask && !mantle) return worn;

        return worn.Where(apparel =>
        {
            if (apparel.def.apparel.layers.Contains(ModDefOf.UmApparelLayerNarehateOverlay)) return true;

            // Same head or body choice as DynamicPawnRenderNodeSetup_Apparel.ProcessApparel.
            var props = apparel.def.apparel;
            var headgear = props.parentTagDef == PawnRenderNodeTagDefOf.ApparelHead ||
                           props.parentTagDef != PawnRenderNodeTagDefOf.ApparelBody &&
                           (props.LastLayer == ApparelLayerDefOf.Overhead || props.LastLayer == ApparelLayerDefOf.EyeCover);
            return !(headgear ? mask : mantle);
        }).ToList();
    }
}
