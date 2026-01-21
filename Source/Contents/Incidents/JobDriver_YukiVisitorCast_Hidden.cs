using RimWorld;
using UnityEngine;
using UranvManosaba.Contents.Comps;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace UranvManosaba.Contents.Incidents;

public class JobDriver_YukiVisitorCast_Hidden : JobDriver
{
    private int _wanderEndTick = -1;
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref _wanderEndTick, "_wanderEndTick", -1);
    }
        
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        // 前往地图边缘
        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);

        // 到达后添加隐身 Hediff
        var invisibleDef = ModDefOf.UmHediffYukiVisitorInvisiblity;
        yield return Toils_General.Do(delegate
        {
            pawn.health?.AddHediff(invisibleDef);
            FleckMaker.ThrowDustPuff(pawn.Position, pawn.Map, 2f);
        });

        // 前往聚会点
        // 注意: 因为我们还要走很远, 建议这里加上 PathEndMode.OnCell
        yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);

        // 在聚会点等待随机时间
        var waitToil = Toils_General.Wait(Rand.Range(120, 300)); 
        waitToil.WithProgressBarToilDelay(TargetIndex.B); 
        yield return waitToil;

        // 释放蓄力效果
        var channelDuration = 900;
        var channelToil = Toils_General.Wait(channelDuration);
        channelToil.WithProgressBarToilDelay(TargetIndex.B);
        channelToil.tickAction = delegate
        {
            if (!pawn.IsHashIntervalTick(90)) return;
            var progress = 1f - ((float)pawn.jobs.curDriver.ticksLeftThisToil / channelDuration);
            var currentScale = Mathf.Lerp(3.0f, 8.0f, progress);
            FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastAreaEffect, currentScale);
            // SoundDefOf.Psycast_CastLoop.PlayOneShot(pawn); 
        };
        yield return channelToil;

        // 释放最终效果
        yield return Toils_General.Do(delegate
        {
            var map = pawn.Map;
            var pos = pawn.Position;
            if (map != null)
            {
                // 中心波动
                FleckMaker.Static(pos, map, FleckDefOf.PsycastAreaEffect, 16f);
                FleckMaker.Static(pos, map, FleckDefOf.PsycastAreaEffect, 10f);
                FleckMaker.Static(pos, map, FleckDefOf.PsycastAreaEffect, 6f);
                //FleckMaker.Static(pawn.Position, map, FleckDefOf.ExplosionFlash, 8f);
                SoundDef.Named("PsychicSootheGlobal").PlayOneShotOnCamera(map);
                // 更新 CompYukiVisitor 状态
                var comp = pawn.GetComp<Comp_YukiVisitor>();
                comp?.Notify_isFinished(pawn, true);
            }
            // 解除隐身
            var invisibleHediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(invisibleDef);
            if (invisibleHediff != null) pawn.health?.RemoveHediff(invisibleHediff);
        });
            
        // 完成后闲逛随机时间
        var wanderDuration = Rand.Range(600, 1800);
            
        var initWanderTimer = new Toil
        {
            initAction = delegate
            {
                if (_wanderEndTick == -1) _wanderEndTick = Find.TickManager.TicksGame + wanderDuration;
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
        yield return initWanderTimer;
            
        var wanderLoopStart = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Instant
        };
        yield return wanderLoopStart;
            
        var wanderMove = new Toil
        {
            initAction = delegate
            {
                var p = pawn;
                var wanderDest = RCellFinder.RandomWanderDestFor(p, p.Position, 5f, null, Danger.None);
                if (wanderDest.IsValid) p.pather.StartPath(wanderDest, PathEndMode.OnCell);
            },
            defaultCompleteMode = ToilCompleteMode.PatherArrival
        };
        yield return wanderMove;
            
        var wanderPause = Toils_General.Wait(Rand.Range(30, 60)); // 停顿 0.5~1秒
        yield return wanderPause;
            
        var wanderCheck = new Toil
        {
            initAction = delegate
            {
                if (Find.TickManager.TicksGame < _wanderEndTick)
                {
                    JumpToToil(wanderLoopStart);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
        yield return wanderCheck;
            
        // 然后瞬间离开
        yield return Toils_General.Do(delegate
        {
            var comp = pawn.GetComp<Comp_YukiVisitor>();
            comp?.YukiLeaveMap();
        });
    }
}