using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Visuals;

public class PawnRenderNode_HairBack : PawnRenderNode_Hair
{
    public PawnRenderNode_HairBack(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
        : base(pawn, props, tree)
    {
    }

    public override Graphic GraphicFor(Pawn pawn)
    {
        var hairDef = pawn.story?.hairDef;
        if (hairDef == null || hairDef.noGraphic || pawn.DevelopmentalStage.Baby() || pawn.DevelopmentalStage.Newborn())
        {
            return null;
        }

        var extension = hairDef.GetModExtension<ModExtension_HairExtra>();
        if (extension == null || extension.texPathBack.NullOrEmpty())
        {
            return null;
        }

        return GraphicDatabase.Get<Graphic_Multi>(extension.texPathBack,
            hairDef.overrideShaderTypeDef?.Shader ?? ShaderDatabase.CutoutHair, Vector2.one, ColorFor(pawn));
    }
}
