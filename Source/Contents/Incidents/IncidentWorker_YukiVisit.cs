using RimWorld;
using UranvManosaba.Contents.Utils;
using Verse;
using Verse.AI.Group;

namespace UranvManosaba.Contents.Incidents;

public class IncidentWorker_YukiVisitor : IncidentWorker
{
    // 允许条件
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        // 地图中有小雪，则禁止
        var map = (Map)parms?.target;
        if (map == null) return false;
        foreach (var p in map.mapPawns.AllHumanlikeSpawned)
        {
            if (p.health.hediffSet.HasHediff(ModDefOf.UmHediffYukiDummy)) return false;
        }
        return base.CanFireNowSub(parms);
    }
    // 初始x16倍概率；发生过魔女残骸神秘空投后，若仍没触发过则x32倍概率；发生过后不再有概率提升
    public override float ChanceFactorNow(IIncidentTarget target)
    {
        var postFactor = 16f;
        var comp = Current.Game.GetComponent<ManosabaGameComponent>();
        if (comp != null)
        {
            if (!comp.isYukiVisited && comp.isCorspeDroped) postFactor = 32f;
            else if (comp.isYukiVisited)postFactor = 1f;
        }
        return postFactor * base.ChanceFactorNow(target);
    }
    
    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        var map = (Map)parms.target;

        // 寻找生成点 (地图边缘)
        if (!RCellFinder.TryFindRandomPawnEntryCell(out var spawnCell, map, CellFinder.EdgeRoadChance_Neutral))
        {
            return false;
        }

        // 确定派系
        var faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.PlayerColony) 
                      ?? Find.FactionManager.RandomNonHostileFaction(false, false, false);
            
        // 生成月代雪 Pawn
        var yuki = YukiGeneralUtils.GenerateYukiPawn();

        // 生成到地图上
        GenSpawn.Spawn(yuki, spawnCell, map);

        // 分配 LordJob
        var chillSpot = GetChillSpot(map);
        var lordJob = new LordJob_VisitColony(faction, chillSpot, Rand.Range(55000,65000)); // 停留约 1 天
        LordMaker.MakeNewLord(faction, lordJob, map, new List<Pawn> { yuki });

        // 发送消息
        var letterLabel = "Manosaba_Incident_YukiVisitor_letterLabel".Translate();
        var letterText = "Manosaba_Incident_YukiVisitor_letterText".Translate(ManosabaMod.YukiNameDef.Named("YUKI"));
        Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, yuki);
        //SendStandardLetter(parms, new LookTargets(yuki));

        // 解锁大魔女图鉴
        var codexDef = DefDatabase<EntityCodexEntryDef>.GetNamed("UmEntryYuki", true);
        if (codexDef is { Discovered: false })
        {
            Find.EntityCodex.SetDiscovered(codexDef);
        }
        return true;
    }

    // 寻找一个聚集点，失败时回退为地图中心附近的随机点
    private static IntVec3 GetChillSpot(Map map) => RCellFinder.TryFindRandomSpotJustOutsideColony(CellFinder.RandomEdgeCell(map), map, out var result) ? result : map.Center;
}