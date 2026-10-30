using Verse;

namespace UranvManosaba.Contents.Visuals;

public class PawnRenderNodeWorker_HairBack : PawnRenderNodeWorker_FlipWhenCrawling
{
    public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
    {
        // 爬行时按实际使用的头发贴图朝向检查可见性。
        if (parms.flipHead) parms.facing = parms.facing.Opposite;
        if (!base.CanDrawNow(node, parms)) return false;

        if (parms.Portrait) return true;
        if (parms.flags.FlagSet(PawnRenderFlags.NoBody)) return false;
        if (parms.bed != null && parms.pawn.RaceProps.Humanlike)
        {
            return parms.bed.def.building.bed_showSleeperBody;
        }
        return true;
    }
}
