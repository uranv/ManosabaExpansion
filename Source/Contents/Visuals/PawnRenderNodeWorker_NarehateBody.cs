using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Visuals;
public class PawnRenderNodeWorker_NarehateBody : PawnRenderNodeWorker_Body
{
    // 没有头且在床上时强制绘制身体，防止床上什么也不画
    public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
    {
        if (!parms.Portrait && parms.bed != null && !parms.pawn.health.hediffSet.HasHead) return true;
        return base.CanDrawNow(node, parms);
    }
}