using System.Reflection;
using HarmonyLib;
using RimWorld;
using UranvManosaba.Contents.MentalStates;
using Verse;
using Verse.AI;

namespace UranvManosaba.Contents.Patches;

// Patch: 全局接管魔女暴走的信件发送逻辑
[HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState")]
public static class Patch_MajyoinshiLetter
{
    private static FieldInfo _pawnField = AccessTools.Field(typeof(MentalStateHandler), "pawn");
    // 拦截原版消息
    public static void Prefix(MentalStateHandler __instance, MentalStateDef stateDef, ref bool transitionSilently, out bool __state)
    {
        __state = false;
        if (stateDef == ModDefOf.UmMentalBreakFactor)
        {
            if (!transitionSilently)
            {
                transitionSilently = true;
                __state = true;
            }
        }
    }
    // 发送自定义消息
    public static void Postfix(MentalStateHandler __instance, MentalStateDef stateDef, bool __result, bool __state)
    {
        if (!__result || !__state) return;
        var pawn = (Pawn)_pawnField.GetValue(__instance);
        if (pawn != null)
        {
            SendCustomLetter(pawn, stateDef);
        }
    }
    private static void SendCustomLetter(Pawn pawn, MentalStateDef stateDef)
    {
        var target = JobGiver_MentalBreakFactor.FindHatedTarget(pawn);
        // 标题
        var targetName = target?.NameShortColored ?? "".Translate();
        var pawnName = pawn.NameShortColored;
        TaggedString labelBase = stateDef.beginLetterLabel ?? "Manosaba_LetterLabel_Break".Translate();
        TaggedString finalLabel;
        finalLabel = labelBase + ": " + pawn.LabelShortCap;
        // 内容
        TaggedString textTemplate = stateDef.beginLetter;
        textTemplate = textTemplate.Trim();
        var text = textTemplate
            .Formatted(pawnName, targetName)
            .AdjustedFor(pawn);

        Find.LetterStack.ReceiveLetter(
            finalLabel, 
            text, 
            stateDef.beginLetterDef ?? LetterDefOf.ThreatSmall, 
            pawn
        );
    }
}