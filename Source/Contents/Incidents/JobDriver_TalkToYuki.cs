using RimWorld;
using UranvManosaba.Contents.Comps;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace UranvManosaba.Contents.Incidents;

public class JobDriver_TalkToYuki : JobDriver
{
    private Pawn Visitor => (Pawn)TargetA.Thing;
    private bool _notified;
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Visitor, job, 1, -1, null, errorOnFailed);
    }
    protected override IEnumerable<Toil> MakeNewToils()
    {
        // 检查可对话
        this.FailOnDespawnedOrNull(TargetIndex.A);
        this.FailOn(() => Visitor.Downed || !Visitor.Awake() || Visitor.InMentalState);
        // 走到目标面前
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        // 执行对话逻辑
        yield return Toils_General.Do(delegate
        {
            if (!_notified)
            {
                _notified = true;
                // 让殖民者看着访客
                pawn.rotationTracker.FaceTarget(Visitor);
                    
                // 获取组件并触发对话
                var comp = Visitor.GetComp<Comp_YukiVisitor>();
                comp?.Notify_SpokenTo(pawn);
                    
                // 通知 Lord 发生过交互, 类似原版 ReceiveMemo("SpokenTo")
                Visitor.GetLord()?.ReceiveMemo("SpokenTo");
            }
        });
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref _notified, "notified", false);
    }
}