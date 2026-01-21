using Verse;

namespace UranvManosaba.Contents.Visuals;

// 修改后的节点类, 适配 XML 定义
public class PawnRenderNode_NarehateMutant : PawnRenderNode
{
    // 构造函数必须匹配基类, 不再需要传入 graphic, 因为 graphic 会由 props.texPath 决定
    public PawnRenderNode_NarehateMutant(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
        : base(pawn, props, tree)
    {
    }

    // 重写 GraphicFor 方法
    //public override Graphic GraphicFor(Pawn pawn)
    //{
    // 方法 A: 直接使用 XML 里定义的 texPath (最简单)
    // 基类 PawnRenderNode 已经有加载 texPath 的逻辑。
    // 如果你的贴图路径是固定的（不分体型）, 直接调用基类即可。
    //return base.GraphicFor(pawn);
    //}
}