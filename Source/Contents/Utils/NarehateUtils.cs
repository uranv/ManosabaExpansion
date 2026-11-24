using RimWorld;
using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Utils;

public static class NarehateUtils
{
    public static void PlayTransformationEffect(Pawn pawn)
    {
        if (!pawn.Spawned) return;

        var map = pawn.Map;
        EffecterDefOf.HarbingerTreeConsume.SpawnMaintained(pawn, map);

        var position = pawn.DrawPos;
        if (!position.ShouldSpawnMotesAt(map)) return;

        ThrowBlackSmoke(position, map, 1.75f, 0f, 0.12f);
        var angle = Rand.Range(0f, 45f);
        for (var i = 0; i < 8; i++)
        {
            ThrowBlackSmoke(position, map, Rand.Range(0.8f, 1.15f),
                angle + i * 45f, Rand.Range(0.85f, 1.45f));
        }
    }

    public static void ThrowBlackSmoke(Vector3 position, Map map, float size, float angle, float speed)
    {
        var smoke = FleckMaker.GetDataStatic(position, map, ModDefOf.UmFleckNarehateSmoke, size);
        smoke.instanceColor = new Color(0.04f, 0.035f, 0.055f);
        smoke.rotationRate = Rand.Range(-24f, 24f);
        smoke.velocityAngle = angle;
        smoke.velocitySpeed = speed;
        map.flecks.CreateFleck(smoke);
    }

    public static void UnequipAll(Pawn pawn)
    {
        if (pawn.inventory == null) return;

        var inventory = pawn.inventory.innerContainer;
        pawn.apparel?.GetDirectlyHeldThings().TryTransferAllToContainer(inventory);
        pawn.equipment?.GetDirectlyHeldThings().TryTransferAllToContainer(inventory);
    }

    public static void UnloadTransformationGear(Pawn pawn)
    {
        if (pawn.inventory == null) return;

        UnequipAll(pawn);
        if (pawn.carryTracker?.CarriedThing is { } cargo && cargo is not Pawn)
        {
            pawn.inventory.innerContainer.TryAddOrTransfer(cargo);
        }

        var position = pawn.PositionHeld;
        if (pawn.MapHeld is { } map && position.InBounds(map))
            pawn.inventory.DropAllNearPawn(position, forbid: true);
    }
}
