using RimWorld;
using UnityEngine;
using UranvManosaba.Contents.Utils;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace UranvManosaba.Contents.PsychicRituals;

public class PsychicRitualToil_Sabbat_Outcome : PsychicRitualToil
{
    private PsychicRitualRoleDef _invokerRole;
    private List<IntVec3> _invokerPositions;
    private PsychicRitualRoleDef _chanterRole;
    private List<IntVec3> _chanterPositions;
    private int _ticksLeft;
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref _invokerRole, "_invokerRole");
        Scribe_Collections.Look(ref _invokerPositions,  "_invokerPositions");
        Scribe_Defs.Look(ref _chanterRole, "_chanterRole");
        Scribe_Collections.Look(ref _chanterPositions,  "_chanterPositions");
        Scribe_Values.Look(ref _ticksLeft, "_ticksLeft", 0);
    }
        
    private Sustainer _musicSustainer;
        
    private readonly int _afterDuration = Rand.Range(450,550);
        
    protected PsychicRitualToil_Sabbat_Outcome()
    {
    }
    public PsychicRitualToil_Sabbat_Outcome(
        PsychicRitualRoleDef invokerRole, IEnumerable<IntVec3> invokerPositions,
        PsychicRitualRoleDef chanterRole, IEnumerable<IntVec3> chanterPositions)
    {
        _invokerRole = invokerRole;
        _invokerPositions = new List<IntVec3>(invokerPositions ?? Enumerable.Empty<IntVec3>());
        _chanterRole = chanterRole;
        _chanterPositions = new List<IntVec3>(chanterPositions ?? Enumerable.Empty<IntVec3>());
    }
        

    public override void Start(Verse.AI.Group.PsychicRitual psychicRitual, PsychicRitualGraph parent)
    {
        base.Start(psychicRitual, parent);
        var comp = Current.Game.GetComponent<ManosabaGameComponent>();
        if (comp is not { cachedSustainer: null })
        {
            _musicSustainer = comp.cachedSustainer;
            if (ManosabaMod.Settings.debugMode) Log.Message("[Manosaba] Sustainer get from comp (PsychicRituals.PsychicRitualToil_Sabbat_Outcome.Start)");
        }
        else Log.Error("[Manosaba] comp has null Sustainer (PsychicRituals.PsychicRitualToil_Sabbat_Outcome.Start)");
            
        ApplyOutcome(psychicRitual);
        TickSound(psychicRitual);
        _ticksLeft = _afterDuration;
    }

    public override bool Tick(Verse.AI.Group.PsychicRitual psychicRitual, PsychicRitualGraph parent)
    {
        base.Tick(psychicRitual, parent);
        _ticksLeft--;
        TickSound(psychicRitual);
        // 等待 Invoke 产生的特效结束
        if (_ticksLeft > 0) return false;
        if (_musicSustainer != null) Sabbat_Sfx.StopSustainer(_musicSustainer);
        psychicRitual.ReleaseAllPawnsAndBuildings(); 
        return true;
    }


    public override void UpdateAllDuties(Verse.AI.Group.PsychicRitual psychicRitual, PsychicRitualGraph parent)
    {
        if (_ticksLeft <= 0) return;

        var targetInfo = (LocalTargetInfo)psychicRitual.assignments.Target;

        if (_invokerRole != null && _invokerPositions != null)
        {
            int num = 0;
            foreach (var pawn in psychicRitual.assignments.AssignedPawns(_invokerRole))
            {
                if (num >= _invokerPositions.Count) break;
                var bestStandableRolePosition3 = psychicRitual.def.GetBestStandableRolePosition(
                    true,
                    _invokerPositions[num++],
                    targetInfo.Cell,
                    psychicRitual.Map);
                SetPawnDuty(
                    pawn,
                    psychicRitual,
                    parent,
                    DutyDefOf.PsychicRitualDance,
                    targetInfo,
                    bestStandableRolePosition3);
            }
        }
        if (_chanterRole != null &&  _chanterPositions != null)
        {
            int num = 0;
            foreach (var pawn in psychicRitual.assignments.AssignedPawns(_chanterRole))
            {
                if (num >= _chanterPositions.Count) break;
                var bestStandableRolePosition3 = psychicRitual.def.GetBestStandableRolePosition(
                    true,
                    _chanterPositions[num++],
                    targetInfo.Cell,
                    psychicRitual.Map);
                SetPawnDuty(
                    pawn,
                    psychicRitual,
                    parent,
                    DutyDefOf.PsychicRitualDance,
                    targetInfo,
                    bestStandableRolePosition3);
            }
        }
    }


    private static void ApplyOutcome(Verse.AI.Group.PsychicRitual psychicRitual)
    {
        if  (psychicRitual?.Map == null) return;
        var map = psychicRitual.Map;
        var pos = psychicRitual.assignments.Target.Cell;
        // 计算最终品质
        var qualityFromToil = 0;
        var comp = Current.Game.GetComponent<ManosabaGameComponent>();
        if (comp != null)
        {
            qualityFromToil = comp.cachedQuality;
            comp.cachedQuality = 0;
            if (ManosabaMod.Settings.debugMode) Log.Message($"[Manosaba] qualityFromToil get {qualityFromToil} (PsychicRituals.PsychicRitualToil_Sabbat_Outcome.ApplyOutcome)");
        }
            
        var ritualPower = psychicRitual.power;
        var qualityOffset = -2 +
                            (ritualPower >= ManosabaMod.Settings.thresholdMinusOne/100f ? 1 : 0) +
                            (ritualPower >= ManosabaMod.Settings.thresholdZero/100f ? 1 : 0) +
                            (ritualPower >= ManosabaMod.Settings.thresholdPlusOne/100f ? 1 : 0);
        var finalQuality = (QualityCategory)Mathf.Clamp(qualityFromToil + qualityOffset, (int)QualityCategory.Awful, (int)QualityCategory.Legendary);
        if (ManosabaMod.Settings.debugMode) Log.Message($"[Manosaba] finial quality is: {finalQuality} {(int)finalQuality} = {qualityFromToil} + {qualityOffset} (PsychicRituals.PsychicRitualToil_Sabbat_Outcome.ApplyOutcome)");
        // 生成 Yuki, 默认征召状态
        var yuki = YukiGeneralUtils.GenerateYukiPawn(true, finalQuality);
        GenSpawn.Spawn(yuki, pos, map);
        if (yuki.drafter != null) yuki.drafter.Drafted = true;
        yuki.health?.AddHediff(ModDefOf.UmHediffYukiSkipDebuff);
        SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(pos, map));
        // 播放最终特效
        EffecterDefOf.MeatExplosion.Spawn(pos, map).Cleanup();
        //var innerExitOverlay = FleckMaker.GetDataAttachedOverlay(_cachedYuki, FleckDefOf.PsycastSkipInnerExit, Vector3.zero);
        //innerExitOverlay.link.detachAfterTicks = 0;
        //_cachedYuki.Map.flecks.CreateFleck(innerExitOverlay);
    }
        
    private void TickSound(Verse.AI.Group.PsychicRitual psychicRitual)
    {
        Find.MusicManagerPlay.ForceSilenceFor(1.0f);
        if (_musicSustainer == null || _musicSustainer.Ended)
        {
            _musicSustainer = Sabbat_Sfx.CreateSustainer(psychicRitual);
            var comp = Current.Game.GetComponent<ManosabaGameComponent>();
            if (comp != null) comp.cachedSustainer = _musicSustainer;
        }
        else
        {
            _musicSustainer.Maintain();
            _musicSustainer.externalParams["MusicVolume"] = Prefs.VolumeMusic;
        }
    }
        
}