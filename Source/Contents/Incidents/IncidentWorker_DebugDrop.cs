using RimWorld;
using UranvManosaba.Contents.Utils;
using Verse;

namespace UranvManosaba.Contents.Incidents;

public class IncidentWorker_DebugDrop : IncidentWorker
{
    // 定义执行逻辑
    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        // 0. 地图检查
        var map = (Map)parms.target;
        if (map == null)
        {
            Log.Error("[Manosaba] Narehate corpse generate failed: Map is null (Incidents.IncidentWorker_DebugDrop.TryExecuteWorker)");
            return false;
        }
        if (!DropCellFinder.TryFindDropSpotNear(map.Center, map, out var dropSpot, false, false, false))
        {
            Log.Error("[Manosaba] Narehate corpse generate failed: Failed to find suitable drop spot (Incidents.IncidentWorker_DebugDrop.TryExecuteWorker)");
            return false;
        }
        // 0.5 解锁前置科技
        ResearchUtils.UnlockResearchPrereqs();
            
        // 1. 生成人类尸体 Pawn
        var specialCorpse = NarehateCorpseGenerator.GenCorpse();

        // 2. 空投新尸体
        var podInfo = new ActiveTransporterInfo();
        podInfo.innerContainer.TryAddOrTransfer(specialCorpse);
        podInfo.openDelay = 180;
        podInfo.leaveSlag = true;

        // 3. 使用 DropPodUtility 生成空投舱
        DropPodUtility.MakeDropPodAt(dropSpot, map, podInfo);

        // 4. 发送消息
        SendStandardLetter(parms, new TargetInfo(dropSpot, map));

        // 5. 遍历当前地图殖民者, 选取一个添加魔女因子 UmHediffHumanDummy
        RandomSelector.TryAddDummyToRandomPawnOnMap(map, inverseTemperature: ManosabaMod.Settings.inverseTemperature);
        return true;
    }
    
    // 永不自然触发
    protected override bool CanFireNowSub(IncidentParms parms) => false;
}