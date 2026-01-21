using HarmonyLib;
using Verse;

namespace UranvManosaba.Contents.Patches;

// Patch: 屠宰魔女尸体类配方添加产物 结晶化眼球
// 拦截 Corpse 类的 ButcherProducts 方法
[HarmonyPatch(typeof(Corpse), "ButcherProducts")]
public static class Patch_Corpse_ButcherProducts
{
    public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, Corpse __instance)
    {
        foreach (var thing in __result)
        {
            yield return thing;
        }

        if (__instance.InnerPawn?.health?.hediffSet == null)
        {
            yield break;
        }

        int crystalEyeCount = 0;
        var hediffs = __instance.InnerPawn.health.hediffSet.hediffs;
        foreach (var t in hediffs)
        {
            if (t.def == ModDefOf.UmHediffCrystallized)
            {
                crystalEyeCount++;
            }
        }
            
        if (crystalEyeCount > 0)
        {
            var extraProductDef = DefDatabase<ThingDef>.GetNamed("UmThingCrystallizedEye", false);

            if (extraProductDef == null) yield break;
            var extraThing = ThingMaker.MakeThing(extraProductDef);
            extraThing.stackCount = crystalEyeCount;
            yield return extraThing;
        }
    }
}