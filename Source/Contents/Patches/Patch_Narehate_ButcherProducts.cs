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
        if (__result != null)
        {
            foreach (var thing in __result)
            {
                yield return thing;
            }
        }
        if (__instance is not { InnerPawn: { } p } || p.health?.hediffSet == null)
        {
            yield break;
        }
        
        var crystalEyeCount = Enumerable.Count(p.health.hediffSet.hediffs, h => h.def == ModDefOf.UmHediffCrystallized);
        if (crystalEyeCount <= 0)
        {
            yield break;
        }
        
        var extraProductDef = ModDefOf.UmThingCrystallizedEye;  //DefDatabase<ThingDef>.GetNamed("UmThingCrystallizedEye", false);
        if (extraProductDef == null)
        {
            yield break;
        }
        
        var stackLimit = extraProductDef.stackLimit;
        while (crystalEyeCount > 0)
        {
            var stack = Math.Min(crystalEyeCount, stackLimit);
            var thing = ThingMaker.MakeThing(extraProductDef);
            thing.stackCount = stack;
            yield return thing;
            crystalEyeCount -= stack;
        }
    }
}