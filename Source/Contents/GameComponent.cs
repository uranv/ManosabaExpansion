using Verse;
using Verse.Sound;

namespace UranvManosaba.Contents;

// 存档全局状态
public class ManosabaGameComponent : GameComponent
{
    // 炙烤药瓶
    public bool IsAburiBinUnlocked;
    // 简易长矛
    public bool IsSimpleSpearUnlocked;
    // 小雪来访
    public bool IsYukiVisited;
    // 尸体空投
    public bool IsCorpseDropped;

    // 仪式缓存
    public int CachedQuality;

    // 运行时 Unity 音频引用，无需持久化
    public Sustainer CachedSustainer;

    public ManosabaGameComponent(Game game)
    {
    }

    // 存档读写
    public override void ExposeData()
    {
        Scribe_Values.Look(ref IsAburiBinUnlocked, "IsAburiBinUnlocked", false);
        Scribe_Values.Look(ref IsSimpleSpearUnlocked, "IsSimpleSpearUnlocked", false);
        Scribe_Values.Look(ref IsYukiVisited, "IsYukiVisited", false);
        Scribe_Values.Look(ref IsCorpseDropped, "IsCorpseDropped", false);
        Scribe_Values.Look(ref CachedQuality,"CachedQuality",0);
    }
}