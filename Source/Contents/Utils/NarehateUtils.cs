using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UranvManosaba.Contents.Utils;

public static class NarehateUtils
{
    /// <summary>
    /// 绘制工具：刷新全部 Render Nodes
    /// </summary>
    public static void RefreshPawnGraphics(Pawn pawn)
    {
        if (pawn == null || pawn.Drawer == null || pawn.Drawer.renderer == null) return;
        pawn.Drawer.renderer.SetAllGraphicsDirty();
        pawn.Drawer.renderer.renderTree.SetDirty();
        PortraitsCache.SetDirty(pawn);
    }
    
    /// <summary>
    /// 魔女残骸变身特效
    /// !!!needfix!!!
    /// 特效需要修改
    /// </summary>
    public static void EffecterNarehateTrans(Pawn pawn)
    {
        if (pawn is not { Spawned: true } || pawn.Map == null) return;

        var pos = pawn.DrawPos;
        var map = pawn.Map;

        // 中心闪光 (瞬间高亮)
        FleckMaker.ThrowLightningGlow(pos, map, 3.0f);
        // 爆发烟雾 (产生气浪感)
        for (var i = 0; i < 6; i++)
        {
            Vector3 offset = Rand.InsideUnitCircle * 0.5f;
            FleckMaker.ThrowAirPuffUp(pos + offset, map);
        }
        // 能量粒子 (向四周飞溅)
        for (var i = 0; i < 12; i++)
        {
            var data = FleckMaker.GetDataStatic(
                pos, 
                map, 
                FleckDefOf.MicroSparks, 
                Rand.Range(1.2f, 2.5f)
            );
            data.velocityAngle = Rand.Range(0, 360);
            data.velocitySpeed = Rand.Range(1f, 4f);
            map.flecks.CreateFleck(data);
        }

        // 音效
        SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(pawn);
    }

    /// <summary>
    /// 脱去所有装备
    /// </summary>
    public static void UnequipAll(Pawn pawn, bool putInInventory = true)
    {
        if (pawn == null) return;
        // 卸下装备
        if (pawn.apparel is { WornApparelCount: > 0 })
        {
            var wornApparel = pawn.apparel.WornApparel.ListFullCopy();
            foreach (var item in wornApparel)
            {
                pawn.apparel.Remove(item);
                if (!pawn.inventory.innerContainer.TryAddOrTransfer(item))
                {
                    GenPlace.TryPlaceThing(item, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                }
            }
        }
        // 武器放回背包
        if (pawn.equipment == null || !pawn.equipment.HasAnything()) return;
        var equipment = pawn.equipment.AllEquipmentListForReading.ListFullCopy();
        foreach (var item in equipment)
        {
            pawn.equipment.Remove(item);
            if (!pawn.inventory.innerContainer.TryAddOrTransfer(item))
            {
                GenPlace.TryPlaceThing(item, pawn.Position, pawn.Map, ThingPlaceMode.Near);
            }
        }
        // 不放入背包则全部掉落周围，默认禁止物品
        if (putInInventory) return;
        pawn.inventory?.DropAllNearPawn(pawn.Position, forbid: true);
    }
}