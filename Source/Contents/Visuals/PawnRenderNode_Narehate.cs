using Verse;

namespace UranvManosaba.Contents.Visuals;

public class PawnRenderNode_Narehate : PawnRenderNode
{
    public enum BodyPart
    {
        Body,
        Head,
    }
    private BodyPart  _bodyPart;
    public PawnRenderNode_Narehate(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree, BodyPart bodyPart)
        : base(pawn, props, tree)
    {
        _bodyPart = bodyPart;
    }
        
    public override Graphic GraphicFor(Pawn pawn)
    {
        if (_bodyPart == BodyPart.Head && pawn.health.hediffSet is { HasHead: false }) return null;
        
        return base.GraphicFor(pawn);
    }
}