using RimWorld;
using UnityEngine;
using UranvManosaba.Contents.Visuals;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class CompProperties_YukiHair : CompProperties
{
    // 正面层（覆盖在脸部/头盔上）
    public string texPathFront;
    public float layerFront = 89.1f;
    public Vector2 drawSizeFront = Vector2.one;
        
    // 背面层（位于身体/装备下方）
    public string texPathBack;
    public float layerBack = -0.1f;
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
            // var hairNodeFront = new PawnRenderNodeProperties
            // {
            //     debugLabel = "YukiHairFront",
            //     texPath = Props.texPathFront,
            //     drawSize = Props.drawSizeFront,
            //     parentTagDef = PawnRenderNodeTagDefOf.Head,
            //     baseLayer = Props.layerFront,
            //     pawnType = PawnRenderNodeProperties.RenderNodePawnType.HumanlikeOnly,
            //     // 使用自定义 Worker 处理可见性和偏移
            //     workerClass = typeof(PawnRenderNodeWorker_HairFront)
            // };
            var hairNodeFront = new PawnRenderNodeProperties
            {
                debugLabel = "YukiHairFront",
                nodeClass = typeof(PawnRenderNode),
                workerClass = typeof(PawnRenderNodeWorker_FlipWhenCrawling),
                shaderTypeDef = ShaderTypeDefOf.Cutout,
                parentTagDef = PawnRenderNodeTagDefOf.Head,
                skipFlag = RenderSkipFlagDefOf.None,
                baseLayer = Props.layerFront,
                overlayLayer = PawnOverlayDrawer.OverlayLayer.Head,
                colorType = PawnRenderNodeProperties.AttachmentColorType.Custom,
                useRottenColor = false,
                useSkinShader = false,
                texPath = Props.texPathFront,
                drawSize = Props.drawSizeFront,
                pawnType = PawnRenderNodeProperties.RenderNodePawnType.HumanlikeOnly,
            };
            nodes.Add(new PawnRenderNode(pawn, hairNodeFront, pawn.Drawer.renderer.renderTree));
        }

        if (!string.IsNullOrEmpty(Props.texPathBack))
        {
            // var hairNodeBack = new PawnRenderNodeProperties
            // {
            //     debugLabel = "YukiHairBack",
            //     texPath = Props.texPathBack,
            //     drawSize = Props.drawSizeBack,
            //     parentTagDef = PawnRenderNodeTagDefOf.Head,
            //     baseLayer = Props.layerBack,
            //     pawnType = PawnRenderNodeProperties.RenderNodePawnType.HumanlikeOnly,
            //     workerClass = typeof(PawnRenderNodeWorker_HairBack)
            // };
            var hairNodeBack = new PawnRenderNodeProperties
            {
                debugLabel = "YukiHairBack",
                workerClass = typeof(PawnRenderNodeWorker_HairBack),
                parentTagDef = PawnRenderNodeTagDefOf.Head,
                skipFlag = RenderSkipFlagDefOf.None,
                baseLayer = Props.layerBack,
                overlayLayer = PawnOverlayDrawer.OverlayLayer.Head,
                colorType = PawnRenderNodeProperties.AttachmentColorType.Custom,
                useRottenColor = false,
                useSkinShader = false,
                texPath = Props.texPathBack,
                drawSize = Props.drawSizeBack,
                pawnType = PawnRenderNodeProperties.RenderNodePawnType.HumanlikeOnly,
                visibleFacing = new List<Rot4>
                {
                    Rot4.South,
                },
            };

            nodes.Add(new PawnRenderNode(pawn, hairNodeBack, pawn.Drawer.renderer.renderTree));
        }

        return nodes;
    }
}