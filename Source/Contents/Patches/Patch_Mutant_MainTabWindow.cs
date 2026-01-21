using HarmonyLib;
using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Patches;

// Patch: 允许控制魔女残骸变种人工作优先级 & 使用方案
public static class Patch_MutantMainTabWindow
{
    public static void AddNarehates(ref IEnumerable<Pawn> resultList)
    {
        if (Find.CurrentMap == null) return;

        var narehates = Find.CurrentMap.mapPawns.AllPawnsSpawned
            .Where(p => p.Faction == Faction.OfPlayer && 
                        p.IsMutant && 
                        p.mutant.Def.defName == "UmMutantNarehate");

        var enumerable = narehates.ToList();
        if (enumerable.Any())
        {
            resultList = resultList.Concat(enumerable).Distinct();
        }
    }
}

// 1. 工作优先级面板 (Work Tab)
[HarmonyPatch(typeof(MainTabWindow_Work), "Pawns", MethodType.Getter)]
public static class Patch_MainTabWindow_Work_Pawns
{
    public static void Postfix(ref IEnumerable<Pawn> __result)
    {
        Patch_MutantMainTabWindow.AddNarehates(ref __result);
    }
}

// 2. 方案分配面板 (Assign Tab)
//[HarmonyPatch(typeof(MainTabWindow_Assign), "Pawns", MethodType.Getter)]
//public static class Patch_MainTabWindow_Assign_Pawns
//{
//public static void Postfix(ref IEnumerable<Pawn> __result)
//{
//Patch_MutantMainTabWindow.AddNarehates(ref __result);
//}
//}