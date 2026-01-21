using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Patches;

// Patch: 为魔女 Race ThingDef 导入 Human 的翻译
[StaticConstructorOnStartup]
public static class Patch_FixYukiTranslations
{
    static Patch_FixYukiTranslations()
    {
        var myPawnDef = ModDefOf.UmThingRaceYuki;

        var humanDef = ThingDefOf.Human;

        if (myPawnDef == null || humanDef == null)
        {
            if (ManosabaMod.Settings.debugMode) Log.Warning("[Manosaba] Translation patch failed: Pawn or Human definition missing. (Patches.Patch_FixYukiTranslations)");
            return;
        }
        if (humanDef.tools == null) return;
        if (myPawnDef.tools != null)
        {
            for (var i = 0; i < myPawnDef.tools.Count; i++)
            {
                if (i >= humanDef.tools.Count) break;

                var myTool = myPawnDef.tools[i];
                var humanTool = humanDef.tools[i];

                myTool.label = humanTool.label;
                myTool.labelNoLocation = humanTool.labelNoLocation;
            }
        }
    }
}