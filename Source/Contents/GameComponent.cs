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

    // 待离场月代雪及是否施加心情；每 tick 末及每帧清空，存档时不会残留
    private readonly Dictionary<Pawn, bool> _pendingDepartures = new();

    public ManosabaGameComponent(Game game)
    {
    }

    public bool IsDepartureQueued(Pawn p) => _pendingDepartures.ContainsKey(p);

    public bool TryQueueDeparture(Pawn p, bool applyMood)
    {
        if (_pendingDepartures.ContainsKey(p)) return false;
        _pendingDepartures.Add(p, applyMood);
        return true;
    }

    // 地图及世界 tick 之后执行；Update 处理暂停时登记的离场
    public override void GameComponentTick() => ProcessDepartures();

    public override void GameComponentUpdate() => ProcessDepartures();

    private void ProcessDepartures()
    {
        if (_pendingDepartures.Count == 0) return;
        var departures = _pendingDepartures.ToList();
        _pendingDepartures.Clear();
        foreach (var departure in departures)
        {
            if (!departure.Key.Destroyed) Utils.YukiGeneralUtils.Depart(departure.Key, departure.Value);
        }
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