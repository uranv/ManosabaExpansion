using HarmonyLib;
using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Patches;

// 修改月代雪 LovinAgeFactor 忽略年龄差
[HarmonyPatch(typeof(Pawn_RelationsTracker), "LovinAgeFactor")]
public static class Patch_LovinAgeFactor_Prefix
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
		__result = 1.0f;
		return false;
	}
}