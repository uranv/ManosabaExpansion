using System.Collections;
using HarmonyLib;
using Verse;

namespace UranvManosaba.Contents.Patches;

[StaticConstructorOnStartup]
internal static class YukiFacialAnimation
{
    static YukiFacialAnimation()
    {
        var facialAnimation = LoadedModManager.RunningModsListForReading.FirstOrDefault(mod =>
            string.Equals(mod.PackageIdPlayerFacing, "Nals.FacialAnimation", StringComparison.OrdinalIgnoreCase));
        var generatorType = facialAnimation?.assemblies.loadedAssemblies
            .Select(assembly => assembly.GetType("FacialAnimation.FaceTypeGenerator`1"))
            .FirstOrDefault(type => type != null);
        if (generatorType == null)
            return;

        // Limit FA's choices for Yuki and keep the original Def instances.
        foreach (var (part, defName) in new[]
                 {
                     ("Brow", "BrowNormal"),
                     ("Mouth", "MouthNormal"),
                     ("Skin", "SkinNormal"),
                     ("Emotion", "EmotionNormal"),
                     ("LidOption", "LidOptionNormal")
                 })
        {
            var defType = generatorType.Assembly.GetType($"FacialAnimation.{part}TypeDef");
            if (defType == null || GenDefDatabase.GetDefSilentFail(defType, defName) is not { } faceType)
                continue;

            var choices = AccessTools.Field(generatorType.MakeGenericType(defType), "raceFaceTypeList");
            if (choices?.GetValue(null) is not IDictionary choicesByRace)
                continue;

            var fixedChoices = Array.CreateInstance(defType, 1);
            fixedChoices.SetValue(faceType, 0);
            choicesByRace[nameof(ModDefOf.UmThingRaceYuki)] = fixedChoices;
        }
    }
}
