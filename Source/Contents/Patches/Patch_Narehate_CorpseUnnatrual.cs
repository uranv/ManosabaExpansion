using HarmonyLib;
using RimWorld;
using UranvManosaba.Contents.Items;
using Verse;

namespace UranvManosaba.Contents.Patches;

// Patch: "魔女残骸尸体"物品分类添加到"异常尸体" Filter
[HarmonyPatch(typeof(SpecialThingFilterWorker_CorpsesUnnatural), "Matches")]
public static class Patch_SpecialThingFilterWorker_CorpsesUnnatural_Matches
{
    public static void Postfix(Thing t, ref bool __result)
    {
        if (__result) return;
        if (t is Corpse_NarehateCorpse)
        {
            __result = true;
        }
    }
}