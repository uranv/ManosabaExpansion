using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Visuals;

[StaticConstructorOnStartup]
public static class NarehateGraphics
{
    public static Graphic NarehateBody;
    public static Graphic NarehateHead;

    //public static Graphic Invisible;

    static NarehateGraphics()
    {
        // 加载身体贴图
        NarehateBody = GraphicDatabase.Get<Graphic_Multi>(
            "things/pawn/humanlike/narehate/body", 
            ShaderDatabase.Cutout, 
            new Vector2(1.5f, 1.5f), // 根据需要调整尺寸
            Color.white
        );

        // 加载头部贴图
        NarehateHead = GraphicDatabase.Get<Graphic_Multi>(
            "things/pawn/humanlike/narehate/head", 
            ShaderDatabase.Cutout, 
            new Vector2(1.5f, 1.5f), 
            Color.white
        );
        // 加载隐形贴图
        //Invisible = GraphicDatabase.Get<Graphic_Single>(
        //    "Things/InvisibleReplace", 
        //    ShaderDatabase.Cutout, 
        //    new Vector2(1f, 1f), 
        //    Color.white
        //);
    }
}