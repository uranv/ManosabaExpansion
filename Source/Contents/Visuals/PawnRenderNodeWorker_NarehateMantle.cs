using Verse;

namespace UranvManosaba.Contents.Visuals;

public class PawnRenderNodeWorker_NarehateMantle : PawnRenderNodeWorker_Body
{
    public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
    {
        return parms.flags.FlagSet(PawnRenderFlags.Clothes) && base.CanDrawNow(node, parms);
    }
}
