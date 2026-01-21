using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace UranvManosaba.Contents.Patches;

// Patch: 阻止复活存在 B水 Hediff 的尸体
// 拦截 JobDriver_Resurrect.Resurrect 方法
[StaticConstructorOnStartup]
public static class Patch_NoResurrect
{
    static Patch_NoResurrect()
    {
        var harmony = new Harmony("uranv.manosaba.patches");
        try
        {
            MethodInfo originalMethod = AccessTools.Method(typeof(JobDriver_Resurrect), "Resurrect");
            MethodInfo patchPrefix = AccessTools.Method(typeof(Patch_JobDriver_Resurrect_Manual), "Prefix");

            if (originalMethod != null)
            {
                harmony.Patch(originalMethod, prefix: new HarmonyMethod(patchPrefix));
                if (ManosabaMod.Settings.debugMode) Log.Message("[Manosaba] Patch Success: JobDriver_Resurrect.Resurrect (Patches.Patch_NoResurrect)");
            }
            else
            {
                Log.Error("[Manosaba] Patch Failed: Unable to find JobDriver_Resurrect.Resurrect (Patches.Patch_NoResurrect)");
            }
        }
        catch (Exception e)
        {
            Log.Error($"[Manosaba] Patch Failed: {e} (Patches.Patch_NoResurrect)");
        }
    }
}
    
public static class Patch_JobDriver_Resurrect_Manual
{
    public static bool Prefix(JobDriver_Resurrect __instance)
    {
        if (__instance.job.GetTarget(TargetIndex.A).Thing is Corpse { InnerPawn: not null } corpse)
        {
            if (!corpse.InnerPawn.health.hediffSet.HasHediff(ModDefOf.UmHediffTredecim)) return true;
            if (ManosabaMod.Settings.debugMode) Log.Warning($"[Manosaba] Block resurrection for {corpse.InnerPawn.LabelShort} (Patches.Patch_JobDriver_Resurrect_Manual)");
            TaggedString text = "Manosaba_Message_NoResurrect".Translate(corpse.InnerPawn.LabelShort);
            Messages.Message(text, corpse.InnerPawn, MessageTypeDefOf.RejectInput);
            // 同时阻止原方法里的 Item.SplitOff(1).Destroy() 销毁物品
            return false;
        }
        return true;
    }
}