using System.Reflection;
using HarmonyLib;
using UranvManosaba.Contents.Visuals;
using Verse;

namespace UranvManosaba.Contents.Patches;

// Patch: 魔女残骸的覆盖绘制视觉效果
// 启动时查找注入所有 PawnRenderNode 子类的 GraphicFor 方法
[HarmonyPatch]
public static class Patch_NarehateVisuals
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        foreach (var type in GenTypes.AllTypes)
        {
            if (typeof(PawnRenderNode).IsAssignableFrom(type) && !type.IsAbstract)
            {
                var method = AccessTools.Method(type, "GraphicFor");
                if (method != null && method.DeclaringType == type)
                {
                    if (ManosabaMod.Settings.debugMode) Log.Message($"[Manosaba] Patch Success: {type.Name} (Patches.Patch_NarehateVisuals.TargetMethods)");
                    yield return method;
                }
            }
        }
    }
    public static bool Prefix(PawnRenderNode __instance, Pawn pawn, ref Graphic __result)
    {
        if (pawn?.health?.hediffSet != null &&
            !pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffNarehate)) return true;

        if (__instance is PawnRenderNode_Narehate) return true;

        __result = null;
        return false;
    }
}