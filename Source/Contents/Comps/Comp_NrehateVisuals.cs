using RimWorld;
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
        if (Pawn == null || !Pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffNarehate))
        {
            return base.CompRenderNodes();
        }

        var list = new List<PawnRenderNode>();

        // 身体配置
        _bodyProps ??= new PawnRenderNodeProperties
        {
            debugLabel = "Narehate_Body",
            tagDef = PawnRenderNodeTagDefOf.Body,
            baseLayer = 12,
            pawnType = PawnRenderNodeProperties.RenderNodePawnType.Any,
            workerClass = typeof(PawnRenderNodeWorker_NarehateBody)
        };
        // 头部配置
        _headProps ??= new PawnRenderNodeProperties
        {
            debugLabel = "Narehate_Head",
            tagDef = PawnRenderNodeTagDefOf.Head,
            baseLayer = 52,
            pawnType = PawnRenderNodeProperties.RenderNodePawnType.Any,
            workerClass = typeof(PawnRenderNodeWorker_NarehateHead),
            parentTagDef = PawnRenderNodeTagDefOf.Body
        };
            
        // 添加节点
        list.Add(new PawnRenderNode_Narehate(Pawn, _bodyProps, Pawn.Drawer.renderer.renderTree, NarehateGraphics.NarehateBody));
        list.Add(new PawnRenderNode_Narehate(Pawn, _headProps, Pawn.Drawer.renderer.renderTree, NarehateGraphics.NarehateHead));
        return list;
    }
}