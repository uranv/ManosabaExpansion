using RimWorld;
using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.PsychicRituals;

public static class Sabbat_Vfx
{
    public static void SpawnParticle(Vector3 center, Map map)
    {
        if (map == null) return;
        
        var moteDef = ModDefOf.UmThingMoteSabbat;
        if (ThingMaker.MakeThing(moteDef) is not MoteThrown_Sabbat mote) return;
        
        mote.Scale = Rand.Range(0.8f, 1.2f);
        mote.instanceColor = new Color(0.9f, 0.1f, 0.1f, 1);
        
        var spawnPos = center + Vector3Utility.RandomHorizontalOffset(0.5f);
        mote.exactPosition = spawnPos;
        
        var velocityX = Rand.Range(-0.25f, 0.25f);
        var velocityZ = Rand.Range(0.03f, 0.33f);
        var velocity = Mathf.Sqrt(velocityX * velocityX + velocityZ * velocityZ);
        var angle = Mathf.Asin(velocityX/velocity) * Mathf.Rad2Deg;
        mote.SetVelocity(angle, velocity);
        
        GenSpawn.Spawn(mote, center.ToIntVec3(), map);
    }
    
    public static void SpawnBloodFilth(Vector3 center, float widthFactor, Map map, float hardBoundary = 3f)
    {
        if (map == null) return;

        // 正态分布范围，约束在 hardBoundary 内部
        var offset = new Vector3
        {
            x = Rand.Gaussian(0f, widthFactor),
            y = 0,
            z = Rand.Gaussian(0f, widthFactor)
        };
        offset = offset.magnitude > hardBoundary ? offset.normalized * (Rand.Value / 2f) : offset;
        var spawnPos = center + offset;
        spawnPos.y += Rand.Range(0f, 0.04f);
        if (!spawnPos.ToIntVec3().InBounds(map))
        {
            //Log.Error("[Sabbat] try to spawn outside of map");
            return;
        }
        var data = FleckMaker.GetDataStatic(
            spawnPos, 
            map, 
            ModDefOf.UmFleckSabbatBlood, 
            Rand.Range(0.8f, 1.2f)
        );
        data.rotation = Rand.Range(0f, 360f);
        data.instanceColor = new Color32(131, 34, 34, 180);
        map.flecks.CreateFleck(data);
    }
}