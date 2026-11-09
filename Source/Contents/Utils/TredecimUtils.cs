using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Utils;

/// <summary>
/// B水处死与禁止复活规则
/// </summary>
public static class TredecimUtils
{
    /// <summary>
    /// 同时具有魔女因子 Dummy 与 B水 Hediff
    /// </summary>
    public static bool HasFactorAndTredecim(Pawn pawn)
    {
        var hediffSet = pawn?.health?.hediffSet;
        return hediffSet != null && hediffSet.HasHediff(ModDefOf.UmHediffTredecim) &&
               (hediffSet.HasHediff(ModDefOf.UmHediffHumanDummy) ||
                hediffSet.HasHediff(ModDefOf.UmHediffMutantDummy));
    }

    /// <summary>
    /// 活着的 Pawn 同时具有两者时立即处死
    /// </summary>
    public static void KillIfFactorAndTredecim(Pawn pawn)
    {
        if (pawn.Dead || !HasFactorAndTredecim(pawn)) return;
        var text = "Manosaba_Message_KilledByTredecim".Translate(pawn.LabelShort);
        Messages.Message(text, pawn, MessageTypeDefOf.NegativeEvent);
        pawn.Kill(null, null);
    }

    /// <summary>
    /// 提示玩家该 Pawn 无法复活
    /// </summary>
    public static void RejectResurrection(Pawn pawn, string source)
    {
        if (ManosabaMod.Settings.DebugMode) Log.Warning($"[Manosaba] Block resurrection for {pawn.LabelShort} ({source})");
        var text = "Manosaba_Message_NoResurrect".Translate(pawn.LabelShort);
        Messages.Message(text, pawn, MessageTypeDefOf.RejectInput, false);
    }
}
