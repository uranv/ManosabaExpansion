using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Visuals;

// 修正 Body worker: 总是绘制, 床上 offset
public class PawnRenderNodeWorker_NarehateBody : PawnRenderNodeWorker
{
    public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
    {
        return true;
    }
    public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
    {
        pivot = Vector3.zero;
        var result = Vector3.zero;
        // if (parms.bed != null)
        // {
        //     result.y = 0.02f;
        // }
        return result;
    }
}
// 修正 Head worker: 总是绘制, 床上 offset
public class PawnRenderNodeWorker_NarehateHead : PawnRenderNodeWorker_Head
{
    public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
    {
        return true;
    }
    public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
    {
        var result = base.OffsetFor(node, parms, out pivot);
        if (parms.bed != null)
        {
            result.y += 0.02f;
        }
        return result;
    }
}