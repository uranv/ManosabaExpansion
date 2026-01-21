using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Incidents;

public class IncidentWorker_MystCargoNarehate:IncidentWorker_GiveQuest
{
    // 若小雪来访过，而此事件没有触发过，概率提升32倍
    public override float ChanceFactorNow(IIncidentTarget target)
    {
        var postFactor = 1f;
        var comp = Current.Game.GetComponent<ManosabaGameComponent>();
        if (comp != null)
        {
            // var lastFireTicks = target.StoryState.lastFireTicks;
            // var isEverFired= lastFireTicks.TryGetValue(def, out var ticks);
            if (comp.isYukiVisited && !comp.isCorspeDroped) postFactor = 32f;
            // Log.Warning($"[Manosaba] lastFireTicks={ticks} and pF={postFactor} (IncidentWorker_MystCargoNarehate)");
        }
        return postFactor * base.ChanceFactorNow(target);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        var result = base.TryExecuteWorker(parms);
        if (result)
        {
            var comp = Current.Game.GetComponent<ManosabaGameComponent>();
            if (comp != null) comp.isCorspeDroped = true;
            //Log.Message("[Manosaba] excuted IncidentWorker_MystCargoNarehate");
        }
        return result;
    }
}