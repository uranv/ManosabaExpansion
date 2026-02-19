using HarmonyLib;
using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Patches;

[HarmonyPatch(typeof(Recipe_ExtractOvum), "AvailableReport")]
public class Patch_Recipe_ExtractOvum_AvailableReport
{
    public static bool Prefix(Thing thing, BodyPartRecord part, ref AcceptanceReport __result )
    {
        // Log.WarningOnce("[Manosaba] Try Patch_Recipe_ExtractOvum_AvailableReport", GenTicks.TicksGame/10 + thing.thingIDNumber);
        if (thing.def != ModDefOf.UmThingRaceYuki)
        {
            return true;
        }
        // Log.WarningOnce("[Manosaba] Found Yuki to Patch_Recipe_ExtractOvum_AvailableReport", GenTicks.TicksGame/10 + thing.thingIDNumber);
        __result = AvailableReport(thing, part);
        return false;
    }

    private static AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
    {
        if (!Find.Storyteller.difficulty.ChildrenAllowed)
        {
            return false;
        }
        if (thing is not Pawn pawn)
        {
            return false;
        }
        if (pawn.gender != Gender.Female)
        {
            return false;
        }
        if (pawn.health.hediffSet.HasHediff(HediffDefOf.PregnantHuman))
        {
            return "CannotPregnant".Translate();
        }
        if (pawn.Sterile())
        {
            return "CannotSterile".Translate();
        }
        if (pawn.health.hediffSet.HasHediff(HediffDefOf.OvumExtracted))
        {
            return "SurgeryDisableReasonOvumExtracted".Translate();
        }
        if (pawn.IsQuestLodger())
        {
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Recipe_Surgery), "AvailableOnNow")]
public class Patch_Recipe_Surgery_AvailableOnNow
{
    public static bool Prefix(Recipe_Surgery __instance, Thing thing, BodyPartRecord part, ref bool __result )
    {
        if (thing.def != ModDefOf.UmThingRaceYuki)
        {
            return true;
        }
        
        if (__instance?.recipe == null)
        {
            return true;
        }
        
        var recipe = __instance.recipe;
        __result = AvailableOnNow(recipe, thing, part);
        return false;
    }

    private static bool AvailableOnNow(RecipeDef recipe, Thing thing, BodyPartRecord part = null)
    {
        if (thing is not Pawn pawn)
        {
            return false;
        }
        if ((recipe.genderPrerequisite ?? pawn.gender) != pawn.gender)
        {
            return false;
        }
        if (recipe.mustBeFertile && pawn.Sterile())
        {
            return false;
        }
        if (!recipe.allowedForQuestLodgers && pawn.IsQuestLodger())
        {
            return false;
        }
        if (recipe.developmentalStageFilter.HasValue && !recipe.developmentalStageFilter.Value.Has(pawn.DevelopmentalStage))
        {
            return false;
        }
        if (ModsConfig.AnomalyActive)
        {
            if (recipe.mutantBlacklist != null)
            {
                return false;
            }
            if (recipe.mutantPrerequisite != null)
            {
                return false;
            }
        }
        return true;
    }
    
}