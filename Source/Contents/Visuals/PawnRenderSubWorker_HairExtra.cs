using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Visuals;

public class PawnRenderSubWorker_HairExtra : PawnRenderSubWorker
{
    public override void TransformScale(PawnRenderNode node, PawnDrawParms parms, ref Vector3 scale)
    {
        var extension = parms.pawn.story?.hairDef?.GetModExtension<ModExtension_HairExtra>();
        if (extension == null) return;

        scale.x *= extension.drawSize.x;
        scale.z *= extension.drawSize.y;
    }

    public override void TransformLayer(PawnRenderNode node, PawnDrawParms parms, ref float layer)
    {
        var extension = parms.pawn.story?.hairDef?.GetModExtension<ModExtension_HairExtra>();
        if (extension == null) return;

        var baseLayer = node is PawnRenderNode_HairBack ? extension.layerBack : extension.layerFront;
        layer += baseLayer - node.Props.baseLayer;
    }
}
