using RimWorld;
using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class CompProperties_YukiHair : CompProperties
{
    // 正面层（覆盖在脸部/头盔上）
    public string texPathFront;
    public float layerFront = 72f;
    public Vector2 drawSizeFront = Vector2.one;
        
    // 背面层（位于身体/装备下方）
    public string texPathBack;
    public float layerBack = 1f;
    public Vector2 drawSizeBack = Vector2.one;

    public Vector2 drawSize = Vector2.one;

    public CompProperties_YukiHair()
    {
        compClass = typeof(Comp_YukiHair);
    }
}
    
public class Comp_YukiHair : ThingComp
{
    private CompProperties_YukiHair Props => (CompProperties_YukiHair)props;

    public override List<PawnRenderNode> CompRenderNodes()
    {
        if (parent is not Pawn pawn) return null;

        List<PawnRenderNode> nodes = new List<PawnRenderNode>();
            
        if (!string.IsNullOrEmpty(Props.texPathFront))
        {
            var hairNodeFront = new PawnRenderNodeProperties
            {
                debugLabel = "YukiHairFront",
                texPath = Props.texPathFront,
                drawSize = Props.drawSizeFront,
                parentTagDef = PawnRenderNodeTagDefOf.Head,
                baseLayer = Props.layerFront,
                pawnType = PawnRenderNodeProperties.RenderNodePawnType.HumanlikeOnly,
                // 使用自定义 Worker 处理可见性和偏移
                workerClass = typeof(PawnRenderNodeWorker_HairFront)
            };

            // 将 offset 数据传递给 node (利用 extra data 或者 worker 内部读取 comp)
            // 这里我们依赖 Worker 去读取 Comp 的数据，所以不需要在这里死板赋值，
            // 但原版 props 不直接支持 Vector3 的自定义传递，除非使用 useGraphic。
            // 简单起见，我们在 Worker 里读取 Comp。

            nodes.Add(new PawnRenderNode(pawn, hairNodeFront, pawn.Drawer.renderer.renderTree));
        }

        if (!string.IsNullOrEmpty(Props.texPathBack))
        {
            var hairNodeBack = new PawnRenderNodeProperties
            {
                debugLabel = "YukiHairBack",
                texPath = Props.texPathBack,
                drawSize = Props.drawSizeBack,
                parentTagDef = PawnRenderNodeTagDefOf.Head,
                baseLayer = Props.layerBack,
                pawnType = PawnRenderNodeProperties.RenderNodePawnType.HumanlikeOnly,
                workerClass = typeof(PawnRenderNodeWorker_HairBack_SouthOnly)
            };

            nodes.Add(new PawnRenderNode(pawn, hairNodeBack, pawn.Drawer.renderer.renderTree));
        }

        return nodes;
    }
}
    
public class PawnRenderNodeWorker_HairBack_SouthOnly : PawnRenderNodeWorker
{
    public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
    {
        if (!base.CanDrawNow(node, parms)) return false;
        if (parms.bed != null)  return false;
        return parms.facing == Rot4.South;
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
    
public class PawnRenderNodeWorker_HairFront : PawnRenderNodeWorker
{
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