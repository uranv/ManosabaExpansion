using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Visuals;

public class PawnRenderNodeWorker_HairBack : PawnRenderNodeWorker_FlipWhenCrawling
{
    public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
    {
        if (parms.Portrait) return true;
        if (parms.bed != null && parms.pawn.RaceProps.Humanlike)
        {
            return parms.bed.def.building.bed_showSleeperBody;
        }
        return base.CanDrawNow(node, parms);
    }
}