using RimWorld;
using UnityEngine;
using UranvManosaba.Contents.Comps;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace UranvManosaba.Contents.Incidents;

public class JobDriver_YukiVisitorCast : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        // 前往指定地点
        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);

        // 等待随机间隔
        var waitToil = Toils_General.Wait(Rand.Range(300, 900)); 
        //waitToil.WithProgressBarToilDelay(TargetIndex.A); // 可选: 显示进度条
        yield return waitToil;

        // 释放蓄力效果
        var channelDuration = 900;
        var channelToil = Toils_General.Wait(channelDuration);
        channelToil.WithProgressBarToilDelay(TargetIndex.B);
        channelToil.tickAction = delegate
        {
            if (pawn.IsHashIntervalTick(90))
            {
                var progress = 1f - ((float)pawn.jobs.curDriver.ticksLeftThisToil / channelDuration);
                var currentScale = Mathf.Lerp(3.0f, 8.0f, progress);
                FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastAreaEffect, currentScale);
                // SoundDefOf.Psycast_CastLoop.PlayOneShot(pawn); 
            }
        };
        yield return channelToil;

        // 释放最终效果
        yield return Toils_General.Do(delegate
        {
            var map = pawn.Map;
            //Vector3 center = pawn.DrawPos;
            // 中心波动
            FleckMaker.Static(pawn.Position, map, FleckDefOf.PsycastAreaEffect, 16f);
            FleckMaker.Static(pawn.Position, map, FleckDefOf.PsycastAreaEffect, 10f);
            FleckMaker.Static(pawn.Position, map, FleckDefOf.PsycastAreaEffect, 6f);
            //FleckMaker.Static(pawn.Position, map, FleckDefOf.ExplosionFlash, 8f);
            SoundDef.Named("PsychicSootheGlobal").PlayOneShotOnCamera(pawn.Map);
            // 更新 CompYukiVisitor 状态
            var comp = pawn.GetComp<Comp_YukiVisitor>();
            comp?.Notify_isFinished(pawn);
        });
    }
}