using HarmonyLib;
using Verse;

namespace UranvManosaba.Contents.Patches;

// Patch: 主动隐藏配方
[HarmonyPatch(typeof(RecipeDef), "AvailableNow", MethodType.Getter)]
public static class Patch_RecipeDef_AvailableNow
{
    public static void Postfix(RecipeDef __instance, ref bool __result)
    {
        if (!__result) return;
            
        // 隐藏炙烤药瓶配方
        if (__instance?.defName == "Make_"+ModDefOf.UmThingAburibin.defName)
        {
            var comp = Current.Game.GetComponent<ManosabaGameComponent>();
            if (comp is { isAburiBinUnlocked: false })
            {
                __result = false;
            }
        }

        // 隐藏简易长矛配方
        if (__instance?.defName == "Make_"+ModDefOf.UmThingWeaponSimpleSpear.defName)
        {
            var comp = Current.Game.GetComponent<ManosabaGameComponent>();
            if (comp is { isSimpleSpearUnlocked: false })
            {
                __result = false;
            }
        }
    }
}