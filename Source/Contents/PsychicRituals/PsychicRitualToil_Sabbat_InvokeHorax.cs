using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace UranvManosaba.Contents.PsychicRituals;

public class PsychicRitualToil_Sabbat_InvokeHorax : PsychicRitualToil_InvokeHorax
{
    [CanBeNull] private Sustainer _musicSustainer;
        
    public PsychicRitualToil_Sabbat_InvokeHorax(
        PsychicRitualRoleDef invokerRole, IEnumerable<IntVec3> invokerPositions,
        PsychicRitualRoleDef targetRole, IEnumerable<IntVec3> targetPositions,
        PsychicRitualRoleDef chanterRole, IEnumerable<IntVec3> chanterPositions,
        PsychicRitualRoleDef defenderRole, IEnumerable<IntVec3> defenderPositions,
        IngredientCount requiredOffering)
        : base(invokerRole, invokerPositions, targetRole, targetPositions, chanterRole, chanterPositions, defenderRole, defenderPositions, requiredOffering)
    {
    }

    public PsychicRitualToil_Sabbat_InvokeHorax()
    {
    }

    // ConsumeRequiredOffering 消耗祭品环节额外保存祭品品质
    public override void ConsumeRequiredOffering(Verse.AI.Group.PsychicRitual psychicRitual)
    {
        if (requiredOffering != null)
        {
            foreach (var item in psychicRitual.assignments.AssignedPawns(invokerRole))
            {
                var thing = item.carryTracker?.CarriedThing;
                if (thing == null || !requiredOffering.filter.Allows(thing)) continue;
                if (thing.TryGetQuality(out var qc))
                {
                    var comp = Current.Game.GetComponent<ManosabaGameComponent>();
                    if (comp != null)
                    {
                        comp.cachedQuality = (int)qc;
                        if (ManosabaMod.Settings.debugMode) Log.Message($"[Manosaba] Captured quality: {qc} (PsychicRituals.PsychicRitualToil_Sabbat_InvokeHorax.ConsumeRequiredOffering)");
                    }
                    else
                    {
                        Log.Error("[Manosaba] GameComponent not found (PsychicRituals.PsychicRitualToil_Sabbat_InvokeHorax.ConsumeRequiredOffering)");
                    }
                }
                break;
            }
        }
        base.ConsumeRequiredOffering(psychicRitual);
    }
        
    // 修复：.ToList()防止集合修改报错
    public override void HoldRequiredOfferings(Verse.AI.Group.PsychicRitual psychicRitual, PsychicRitualGraph parent)
    {
        if (requiredOffering == null) return;
        foreach (var item in psychicRitual.assignments.AssignedPawns(invokerRole))
        {
            var inventorySnapshot = item.inventory.GetDirectlyHeldThings().ToList();
            foreach (var item2 in inventorySnapshot)
            {
                if (requiredOffering.filter.Allows(item2))
                {
                    item.inventory.innerContainer.TryTransferToContainer(
                        item2, item.carryTracker.innerContainer,
                        Mathf.CeilToInt(requiredOffering.GetBaseCount())
                    );
                }
            }
        }
    }

    public override bool Tick(Verse.AI.Group.PsychicRitual psychicRitual, PsychicRitualGraph parent)
    {
        TickVfx(psychicRitual);
        TickSound(psychicRitual);
        base.Tick(psychicRitual, parent);
        return TicksLeft <= 0;
    }
        
    private void TickVfx(Verse.AI.Group.PsychicRitual psychicRitual)
    {
        var percentage = TicksSinceStarted / (hoursUntilOutcome * 2500f);
        if (Rand.Chance(ChanceParticle(percentage)))
        {
            Sabbat_Vfx.SpawnParticle(psychicRitual.assignments.Target.CenterVector3, psychicRitual.Map);
        }
        if (Rand.Chance(ChanceFilth(percentage)))
        {
            Sabbat_Vfx.SpawnBloodFilth(psychicRitual.assignments.Target.CenterVector3, 0.707f, psychicRitual.Map);
        }
        return;

        float ChanceFilth(float p) =>  0.05f * Mathf.Pow(3f * p,2) * Mathf.Exp(1f - Mathf.Pow(3f * p,2));
        float ChanceParticle (float p)=> 1f/(int)(60 - 59 * Math.Pow(p,3));
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