using RimWorld;
using UranvManosaba.Contents.Utils;
using Verse;

namespace UranvManosaba.Contents.Incidents;

public class QuestPart_MystCargoNarehate_Unlock : QuestPart
{

    public string inSignal;
    
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
    }

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        // 解锁前置科技【魔女因子】及其图鉴
        if (signal.tag == inSignal)
        {
            ResearchUtils.UnlockResearchPrereqs();
        }
        // 解锁【魔女残骸】图鉴定义
        var codexDef = DefDatabase<EntityCodexEntryDef>.GetNamed("UmEntryMutant");
        if (codexDef is { Discovered: false })
        {
            Find.EntityCodex.SetDiscovered(codexDef);
        }
        // 选取殖民者添加一个魔女因子
        //Core.Manosaba_Utils.TryAddDummyToRandomPawnOnMap(_pawn.Map, inverseTemperature: ManosabaMod.Settings.inverseTemperature);
    }
}