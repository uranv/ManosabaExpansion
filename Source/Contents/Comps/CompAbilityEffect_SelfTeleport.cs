using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class CompProperties_SelfTeleport : CompProperties_EffectWithDest
{
    public CompProperties_SelfTeleport()
    {
        compClass = typeof(CompAbilityEffect_SelfTeleport);
    }
}
public class CompAbilityEffect_SelfTeleport : CompAbilityEffect
{
    public static string SkipUsedSignalTag = "CompAbilityEffect.SkipUsed";

    private new CompProperties_SelfTeleport Props => (CompProperties_SelfTeleport)props;

    public override IEnumerable<PreCastAction> GetPreCastActions()
    {
        // 起点特效：施法前5tick
        yield return new PreCastAction
        {
            action = delegate(LocalTargetInfo t, LocalTargetInfo arg2)
            {
                var caster = parent.pawn;
                if (caster != null)
                {
                    EffecterDefOf.MeatExplosion.Spawn(caster.Position, caster.Map).Cleanup();
                    EffecterDefOf.MeatExplosion.Spawn(caster.Position, caster.Map).Cleanup();
                }
            },
            ticksAwayFromCast = 5
        };
        // 终点特效：施法时
        yield return new PreCastAction
        {
            action = delegate(LocalTargetInfo t, LocalTargetInfo arg2)
            {
                EffecterDefOf.MeatExplosion.Spawn(t.Cell, parent.pawn.Map).Cleanup();
                EffecterDefOf.MeatExplosion.Spawn(t.Cell, parent.pawn.Map).Cleanup();
            },
            ticksAwayFromCast = 1
        };
    }

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        if (!target.IsValid)
        {
            return;
        }
            
        var destCell = target.Cell;
        var pawn = parent.pawn;
        var map = pawn.Map;
        // 若可唤醒，则唤醒 pawn
        pawn.TryGetComp<CompCanBeDormant>()?.WakeUp();
            
        pawn.Position = destCell;
        pawn.health?.AddHediff(ModDefOf.UmHediffYukiSkipDebuff);
            
        // 移除目标附近迷雾
        if ((pawn.Faction == Faction.OfPlayer || pawn.IsPlayerControlled) && pawn.Position.Fogged(map))
        {
            FloodFillerFog.FloodUnfog(pawn.Position, map);
        }
        // 信号
        pawn.Notify_Teleported();
        SendSkipUsedSignal(pawn.Position, pawn);
        // 噪音
        if (Props.destClamorType != null)
        {
            GenClamor.DoClamor(pawn, destCell, Props.destClamorRadius, Props.destClamorType);
        }
    }
        
    public override bool Valid(LocalTargetInfo target, bool showMessages = false)
    {
        if (!target.Cell.Walkable(parent.pawn.Map))
        {
            return false;
        }
        if (target.Cell.Fogged(parent.pawn.Map))
        {
            return false;
        }
        if (!target.Cell.Standable(parent.pawn.Map)) 
        {
            return false;
        }
        return base.Valid(target, showMessages);
    }

    public static void SendSkipUsedSignal(LocalTargetInfo target, Thing initiator)
    {
        Find.SignalManager.SendSignal(new Signal(SkipUsedSignalTag, target.Named("POSITION"), initiator.Named("SUBJECT")));
    }
}