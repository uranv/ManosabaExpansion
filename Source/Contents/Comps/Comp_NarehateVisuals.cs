using RimWorld;
using UnityEngine;
using UranvManosaba.Contents.Visuals;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class CompProperties_NarehateVisuals : CompProperties
{
    public CompProperties_NarehateVisuals()
    {
        compClass = typeof(Comp_NarehateVisuals);
    }
}
public class Comp_NarehateVisuals : ThingComp
{
    private PawnRenderNodeProperties _bodyProps;
    private PawnRenderNodeProperties _headProps;
    private Pawn Pawn => (Pawn)parent;

    public override List<PawnRenderNode> CompRenderNodes()
    {
        if (Pawn?.health?.hediffSet == null || !Pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffNarehate))
        {
            return base.CompRenderNodes();
        }

        var list = new List<PawnRenderNode>();

        // 身体配置
        // _bodyProps ??= new PawnRenderNodeProperties
        // {
        //     debugLabel = "Narehate_Body",
        //     tagDef = PawnRenderNodeTagDefOf.Body,
        //     baseLayer = 0.1f,
        //     pawnType = PawnRenderNodeProperties.RenderNodePawnType.Any,
        //     workerClass = typeof(PawnRenderNodeWorker_NarehateBody)
        // };
        _bodyProps ??= new PawnRenderNodeProperties
        {
            workerClass = typeof(PawnRenderNodeWorker_NarehateBody),
            nodeClass = typeof(PawnRenderNode_Narehate),
            tagDef = PawnRenderNodeTagDefOf.Body,
            texPath = "things/pawn/humanlike/narehate/body",
            pawnType = PawnRenderNodeProperties.RenderNodePawnType.Any,
            color = Color.white,
            useRottenColor = false,
            useSkinShader = false,
            colorType = PawnRenderNodeProperties.AttachmentColorType.Custom,
            baseLayer = 0.1f,
            overlayLayer = PawnOverlayDrawer.OverlayLayer.Body,
            drawSize = Vector2.one,
            debugLabel = "Narehate_Body",
        };
        // 头部配置
        // _headProps ??= new PawnRenderNodeProperties
        // {
        //     debugLabel = "Narehate_Head",
        //     tagDef = PawnRenderNodeTagDefOf.Head,
        //     baseLayer = 50.1f,
        //     pawnType = PawnRenderNodeProperties.RenderNodePawnType.Any,
        //     workerClass = typeof(PawnRenderNodeWorker_NarehateHead),
        //     parentTagDef = PawnRenderNodeTagDefOf.Body
        // };
        _headProps ??= new PawnRenderNodeProperties
        {
            workerClass = typeof(PawnRenderNodeWorker_NarehateHead),
            nodeClass = typeof(PawnRenderNode_Narehate),
            tagDef = PawnRenderNodeTagDefOf.Head,
            // parentTagDef = PawnRenderNodeTagDefOf.Body,
            texPath = "things/pawn/humanlike/narehate/head",
            pawnType = PawnRenderNodeProperties.RenderNodePawnType.Any,
            color = Color.white,
            useRottenColor = false,
            useSkinShader = false,
            colorType = PawnRenderNodeProperties.AttachmentColorType.Custom,
            baseLayer = 50.1f,
            overlayLayer = PawnOverlayDrawer.OverlayLayer.Head,
            drawSize = Vector2.one,
            debugLabel = "Narehate_Head",
        };
            
        // 添加节点
        list.Add(new PawnRenderNode_Narehate(Pawn, _bodyProps, Pawn.Drawer.renderer.renderTree, PawnRenderNode_Narehate.BodyPart.Body));
        list.Add(new PawnRenderNode_Narehate(Pawn, _headProps, Pawn.Drawer.renderer.renderTree, PawnRenderNode_Narehate.BodyPart.Head));
        return list;
    }
}