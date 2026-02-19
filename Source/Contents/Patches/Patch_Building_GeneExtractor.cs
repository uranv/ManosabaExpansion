using HarmonyLib;
using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Patches;
// Prefix 临时修改魔女因子超凡点数，使其加入抽选池 v1.0.6
[HarmonyPatch(typeof(Building_GeneExtractor), "Finish")]
public static class Patch_Building_GeneExtractor_Finish
{
    private static int _originalArcValue = -1;
    
    public static void Prefix()
    {
        var targetGene = ModDefOf.UmGeneFactor;
        if (targetGene == null)
        {
            Log.ErrorOnce("[Manosaba] GeneDef \"UmGeneFactor\" not found (Patch_Building_GeneExtractor_Finish)",
                Gen.HashCombine(Find.World.info.Seed, "UmGeneFactor"));
            return;
        }
        _originalArcValue = targetGene.biostatArc;
        targetGene.biostatArc = 0;
    }

    public static void Finalizer(Exception __exception)
    {
        var targetGene = ModDefOf.UmGeneFactor;
        if (targetGene == null)
        {
            Log.ErrorOnce("[Manosaba] GeneDef \"UmGeneFactor\" not found (Patch_Building_GeneExtractor_Finish)",
                Gen.HashCombine(Find.World.info.Seed, "UmGeneFactor"));
            return;
        }
        if (_originalArcValue == -1)
        {
            return;
        }
        targetGene.biostatArc = _originalArcValue;
        _originalArcValue = -1;
    }
}