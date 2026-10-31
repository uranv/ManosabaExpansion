using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UranvManosaba.Contents.Utils;
using Verse;

namespace UranvManosaba.Contents.Patches;

[HarmonyPatch(typeof(Recipe_ExtractOvum), nameof(Recipe_ExtractOvum.AvailableReport),
    new[] { typeof(Thing), typeof(BodyPartRecord) })]
public static class Patch_Recipe_ExtractOvum_AvailableReport
{
    public static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
    {
        return YukiSurgeryAgeTranspiler.Apply(instructions, __originalMethod, false);
    }
}

[HarmonyPatch(typeof(Recipe_Surgery), nameof(Recipe_Surgery.AvailableOnNow),
    new[] { typeof(Thing), typeof(BodyPartRecord) })]
public static class Patch_Recipe_Surgery_AvailableOnNow
{
    public static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
    {
        return YukiSurgeryAgeTranspiler.Apply(instructions, __originalMethod, true);
    }
}

internal static class YukiSurgeryAgeTranspiler
{
    private static readonly FieldInfo AgeTracker = AccessTools.Field(typeof(Pawn), nameof(Pawn.ageTracker));
    private static readonly FieldInfo Recipe = AccessTools.Field(typeof(RecipeWorker), nameof(RecipeWorker.recipe));
    private static readonly FieldInfo MinimumAge = AccessTools.Field(typeof(RecipeDef), nameof(RecipeDef.minAllowedAge));
    private static readonly MethodInfo BiologicalAge = AccessTools.PropertyGetter(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.AgeBiologicalYears));
    private static readonly MethodInfo IsYuki = AccessTools.Method(typeof(YukiGeneralUtils), nameof(YukiGeneralUtils.IsYuki));
    private static readonly MethodInfo AgeArgument = AccessTools.Method(typeof(NamedArgument), "op_Implicit", new[] { typeof(int) });
    private static readonly MethodInfo TranslateAge = AccessTools.Method(typeof(TranslatorFormattedStringExtensions), nameof(TranslatorFormattedStringExtensions.Translate),
        new[] { typeof(string), typeof(NamedArgument) });
    private static readonly MethodInfo AgeReport = AccessTools.Method(typeof(AcceptanceReport), "op_Implicit", new[] { typeof(TaggedString) });

    public static IEnumerable<CodeInstruction> Apply(
        IEnumerable<CodeInstruction> instructions, MethodBase originalMethod, bool isSurgery)
    {
        var code = instructions.ToList();
        if (AgeTracker == null || Recipe == null || MinimumAge == null || BiologicalAge == null || IsYuki == null
            || (!isSurgery && (AgeArgument == null || TranslateAge == null || AgeReport == null)))
        {
            return Fail(code, originalMethod, "required age-check fields or methods were not found");
        }

        var comparison = new[]
        {
            new CodeMatch(instruction => instruction.IsLdloc()),
            new CodeMatch(OpCodes.Ldfld, AgeTracker),
            new CodeMatch(OpCodes.Callvirt, BiologicalAge),
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, Recipe),
            new CodeMatch(OpCodes.Ldfld, MinimumAge),
            new CodeMatch(instruction => instruction.opcode == OpCodes.Bge || instruction.opcode == OpCodes.Bge_S)
        };
        var matches = new List<int>();
        var matcher = new CodeMatcher(code);
        while (matcher.MatchStartForward(comparison).IsValid)
        {
            matches.Add(matcher.Pos);
            matcher.Advance(1);
        }
        if (matches.Count != 1)
        {
            return Fail(code, originalMethod, $"expected exactly one age comparison, found {matches.Count}");
        }

        var start = matches[0];
        var rejection = start + comparison.Length;
        var rejectionPattern = isSurgery
            ? new[]
            {
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Ret)
            }
            : new[]
            {
                new CodeMatch(OpCodes.Ldstr, "CannotMustBeAge"),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, Recipe),
                new CodeMatch(OpCodes.Ldfld, MinimumAge),
                new CodeMatch(OpCodes.Call, AgeArgument),
                new CodeMatch(OpCodes.Call, TranslateAge),
                new CodeMatch(OpCodes.Call, AgeReport),
                new CodeMatch(OpCodes.Ret)
            };
        var continuation = rejection + rejectionPattern.Length;
        if (!MatchesAt(code, rejection, rejectionPattern)
            || continuation >= code.Count
            || code[rejection - 1].operand is not Label target
            || !code[continuation].labels.Contains(target)
            || code.Sum(instruction => instruction.labels.Count(label => label.Equals(target))) != 1)
        {
            return Fail(code, originalMethod, "the age rejection or its continuation did not match the expected control flow");
        }

        var first = start;
        if (isSurgery)
        {
            // Harmony may assign different labels to the two jumps that reach the same continuation.
            first -= 5;
            if (!MatchesAt(code, first,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, Recipe),
                    new CodeMatch(OpCodes.Ldfld, MinimumAge),
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(instruction =>
                        (instruction.opcode == OpCodes.Ble || instruction.opcode == OpCodes.Ble_S)
                        && instruction.operand is Label guardTarget
                        && code[continuation].labels.Contains(guardTarget)
                        && code.Sum(item => item.labels.Count(label => label.Equals(guardTarget))) == 1)))
            {
                return Fail(code, originalMethod, "the minimum-age guard did not match the expected control flow");
            }
        }

        // Interior labels can introduce other entry paths; exception boundaries can make the new branch invalid.
        for (var i = first; i <= continuation; i++)
        {
            if (code[i].blocks.Count != 0 || (i > first && i < continuation && code[i].labels.Count != 0))
            {
                return Fail(code, originalMethod, "the age-check block contains an unexpected label or exception boundary");
            }
        }

        // Only the underage path reaches this insertion. Keep every original instruction and label intact.
        code.InsertRange(rejection, new[]
        {
            new CodeInstruction(code[start].opcode, code[start].operand),
            new CodeInstruction(OpCodes.Call, IsYuki),
            new CodeInstruction(OpCodes.Brtrue, target)
        });
        return code;
    }

    private static bool MatchesAt(List<CodeInstruction> code, int index, params CodeMatch[] pattern)
    {
        return index >= 0 && index + pattern.Length <= code.Count
            && new CodeMatcher(code).Advance(index).MatchStartForward(pattern).Pos == index;
    }

    private static IEnumerable<CodeInstruction> Fail(List<CodeInstruction> code, MethodBase method, string reason)
    {
        Log.Error($"[Manosaba] Yuki surgery age exemption failed for {method.DeclaringType?.FullName}.{method.Name}: {reason}. "
            + "Incoming IL was not changed; this mod's age exemption is disabled for this method.");
        return code;
    }
}
