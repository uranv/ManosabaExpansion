using Verse;
using Verse.Sound;

namespace UranvManosaba.Contents;

// 存档全局状态
public class ManosabaGameComponent : GameComponent
{
    // 炙烤药瓶
    public bool isAburiBinUnlocked;
    // 简易长矛
    public bool isSimpleSpearUnlocked;
    // 小雪来访
    public bool isYukiVisited;
    // 尸体空投
    public bool isCorspeDroped;
        
    // 仪式缓存
    public int cachedQuality;
    public Sustainer cachedSustainer;

    public ManosabaGameComponent(Game game)
    {
    }

    // 存档读写
    public override void ExposeData()
    {
        Scribe_Values.Look(ref isAburiBinUnlocked, "isAburiBinUnlocked", false);
        Scribe_Values.Look(ref isSimpleSpearUnlocked, "isSimpleSpearUnlocked", false);
        Scribe_Values.Look(ref isYukiVisited, "isYukiVisited", false);
        Scribe_Values.Look(ref isCorspeDroped, "isCorspeDroped", false);
        Scribe_Values.Look(ref cachedQuality,"cachedQuality",0);
    }
}