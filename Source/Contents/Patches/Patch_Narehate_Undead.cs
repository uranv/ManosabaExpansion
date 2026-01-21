using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace UranvManosaba.Contents.Patches;

// Patch: 阻止倒地死亡
[HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDead")]
public static class Patch_ShouldBeDead
{
    public static bool Prefix(Pawn ___pawn, ref bool __result)
    {
        if (___pawn?.health?.hediffSet == null) return true;
        // 阻止魔女残骸倒地死亡
        if (___pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffNarehate) &&
            !___pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffTredecim))
        {
            __result = false;
            return false;
        }
        // 阻止月代雪倒地死亡
        //if (___pawn.health.hediffSet.HasHediff(Anomaly.ModDefOf.UmHediffYukiVisitorHidden) ||
        //    ___pawn.health.hediffSet.HasHediff(Anomaly.ModDefOf.UmHediffYukiColonistHidden) )
        //{
        //    __result = false;
        //    return false;
        //}
        return true;
    }
}

// Patch: 拦截 Kill()
[HarmonyPatch(typeof(Pawn), "Kill")]
public static class Patch_Pawn_Kill
{
    public static bool Prefix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit)
    {
            
        if (__instance?.health?.hediffSet == null) return true;
            
        var hasHumanDummy = __instance.health.hediffSet.HasHediff(ModDefOf.UmHediffHumanDummy);
        var hasMutantDummy = __instance.health.hediffSet.HasHediff(ModDefOf.UmHediffMutantDummy);
        var hasYukiDummy = __instance.health.hediffSet.HasHediff(ModDefOf.UmHediffYukiDummy);
            
        var hasNarehate = __instance.health.hediffSet.HasHediff(ModDefOf.UmHediffNarehate)||
                          __instance.health.hediffSet.HasHediff(ModDefOf.UmHediffNarehateHidden);
        var hasPotion = __instance.health.hediffSet.HasHediff(ModDefOf.UmHediffTredecim);
        // 月代雪：判定死亡时不死直接离开地图
        if (hasYukiDummy)
        {
            if (ManosabaMod.Settings.debugMode) Log.Warning($"[Manosaba] Blocked Kill() command for {__instance.LabelShort} (Patches.Patch_Pawn_Kill)");
            // 发信
            var letterLabel = "Manosaba_PatchUndead_teleportLeaveOnDeath_letterLabel".Translate(ManosabaMod.YukiNameDef.Named("YUKI"));
            var letterText = "Manosaba_PatchUndead_teleportLeaveOnDeath_letterText".Translate(ManosabaMod.YukiNameDef.Named("YUKI"));
            // 无地图时直接离开
            if (__instance is { Map: null })
            {
                Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent);
                __instance.GetLord()?.Notify_PawnLost(__instance, PawnLostCondition.LeftVoluntarily, null);
                __instance.DeSpawn();
            }
            // 有地图时放下全部装备，播放特效并离开
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, new TargetInfo(__instance.Position, __instance.Map));
            if (__instance.IsColonist) Utils.NarehateUtils.UnequipAll(__instance, false);
            var mode = __instance.IsColonist ? DestroyMode.KillFinalize : DestroyMode.Vanish;
            Utils.YukiGeneralUtils.LeaveMapInstantly(__instance, mode);
            return false;
        }
            
        // 拥有 魔女残骸 Hediff, 且没有 B水 Hediff, 则阻止死亡指令
        if (hasNarehate && !hasPotion )
        {
            if (ManosabaMod.Settings.debugMode) Log.Warning($"[Manosaba] Blocked Kill() command for {__instance.LabelShort} (Patches.Patch_Pawn_Kill)");
            return false; 
        }
            
        // 拥有 B水 Hediff 且拥有魔女因子, 则死亡判定时额外效果触发
        if (!hasPotion || (!hasHumanDummy && !hasMutantDummy)) return true;
        // 添加 晶体化眼 Hediff
        if (!__instance.health.hediffSet.HasHediff(ModDefOf.UmHediffCrystallized))
        {
            Utils.MiscUtils.AddHediffToNaturalEyes(__instance, ModDefOf.UmHediffCrystallized);
        }
        // 并尝试转移 dummy Hediff 到殖民者之一
        Utils.RandomSelector.TryAddDummyToRandomPawnOnMap(__instance.Map, 1, ManosabaMod.Settings.inverseTemperature);
        return true;
    }
}
    
// Patch: 阻止魔女残骸倒地时持续起身的消息轰炸
[HarmonyPatch(typeof(Messages), "Message", new[] { typeof(string), typeof(LookTargets), typeof(MessageTypeDef), typeof(bool) })]
public static class Patch_SilenceUndownedMessage
{
    public static bool Prefix(string text, LookTargets lookTargets, MessageTypeDef def)
    {
        if (def != MessageTypeDefOf.PositiveEvent) return true;
        if (lookTargets.targets is not { Count: > 0 }) return true;
        foreach (var target in lookTargets.targets)
        {
            if (target.Thing is not Pawn pawn ||
                !pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffNarehate)) continue;
            // 检查消息内容: 原版Key "MessageNoLongerDowned", 使用 Translate 键值匹配
            string noLongerDownedText = "MessageNoLongerDowned".Translate(pawn.LabelCap, pawn);
            if (text == noLongerDownedText) return false; // 阻止起身消息发送
        }
        return true;
    }
}