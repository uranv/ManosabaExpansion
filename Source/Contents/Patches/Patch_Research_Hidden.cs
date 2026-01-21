using HarmonyLib;
using Verse;

namespace UranvManosaba.Contents.Patches;

// Patch: 条件隐藏研究项目
[HarmonyPatch(typeof(ResearchProjectDef), "get_IsHidden")]
public static class Patch_ResearchVisibility
{
    public static void Postfix(ResearchProjectDef __instance, ref bool __result)
    {
        if (__result) return;
        // 隐藏前置科技
        bool finishPre = __instance == ModDefOf.UmResearchProjectPrereqs && !__instance.IsFinished;

        // 隐藏【检查魔女的方法】
        bool unlockA = __instance == ModDefOf.UmResearchProjectDetect && !__instance.PrerequisitesCompleted;

        // 隐藏【杀死魔女的方法】
        //bool unlockB = __instance == ModDefOf.UmResearchProjectKill &&(ModDefOf.UmResearchProjectDetect == null || !ModDefOf.UmResearchProjectDetect.IsFinished);

        // 隐藏【控制魔女的方法】
        //bool unlockC = __instance == ModDefOf.UmResearchProjectControl &&(ModDefOf.UmResearchProjectDetect == null || !ModDefOf.UmResearchProjectDetect.IsFinished);

        // 隐藏【魔女因子基因】
        bool unlockD = __instance == ModDefOf.UmResearchProjectGene &&
                       (ModDefOf.UmResearchProjectKill == null || !ModDefOf.UmResearchProjectKill.IsFinished) &&
                       (ModDefOf.UmResearchProjectControl == null || !ModDefOf.UmResearchProjectControl.IsFinished);

        // 隐藏【魔女安息仪式】
        //bool unlockE = __instance == Anomaly.ModDefOf.UmResearchProjectSabbat && !__instance.PrerequisitesCompleted;
            
        //if (finishPre || unlockA || unlockB || unlockC || unlockD || unlockE) __result = true;
        if (finishPre || unlockA || unlockD) __result = true;
    }
}