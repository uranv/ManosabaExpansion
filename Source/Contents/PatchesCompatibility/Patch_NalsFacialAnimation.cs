using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.PatchesCompatibility;

[HarmonyPatch("FacialAnimation.NLFacialAnimationMasterNodeWorker", "GetFinalizedMaterial")]
public static class Patch_FacialAnimation
{
    static bool Prepare()
    {
        var result = ModsConfig.IsActive("Nals.FacialAnimation");
        if (ManosabaMod.Settings.debugMode) Log.Warning($"[Manosaba] {(result?"Success":"Failed")} patch mod Nals.FacialAnimation (PatchesCompatibility.Patch_FacialAnimation.Prepare)");
        return result;
    }
        
    static bool Prefix(object node, object parms, ref Material __result)
    {
        if (node is not PawnRenderNode renderNode) return true;
            
        if (parms == null) return true;
            
        var p = renderNode.tree?.pawn;


        // if (p?.def == Anomaly.ModDefOf.UmThingRaceYuki)
        // {
        //     __result = null;
        //     return false;
        // }

        if (p is not { IsMutant: true } || p.mutant.Def != ModDefOf.UmMutantNarehate)
            return true;
            
        __result = null;
        return false;

    }
}