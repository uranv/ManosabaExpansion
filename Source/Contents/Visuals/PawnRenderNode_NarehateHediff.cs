using Verse;

namespace UranvManosaba.Contents.Visuals;

public class PawnRenderNode_Narehate : PawnRenderNode
{
    private Graphic _overrideGraphic;
        
    public PawnRenderNode_Narehate(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, Graphic graphic)
        : base(pawn, props, tree)
    {
        _overrideGraphic = graphic;
    }
        
    public override Graphic GraphicFor(Pawn pawn)
    {
        return _overrideGraphic;
    }
}