using HarmonyLib;
using RimWorld;
using UranvManosaba.Contents.Utils;
using Verse;
using Verse.AI;

namespace UranvManosaba.Contents.Patches;

// 阻止复活同时具有魔女因子与 B水 Hediff 的尸体
// 只拦截各复活来源的入口，不拦截共同入口 ResurrectionUtility，保留开发者复活工具

// Patch: 普通／食尸鬼复活血清选择目标
// 只限制带有复活效果的物品，其他尸体目标道具不受影响
[HarmonyPatch(typeof(CompTargetable), nameof(CompTargetable.ValidateTarget))]
public static class Patch_CompTargetable_ValidateTarget
{
    public static void Postfix(CompTargetable __instance, LocalTargetInfo target, bool showMessages, ref bool __result)
    {
        if (!__result || target.Thing is not Corpse corpse ||
            !__instance.parent.HasComp<CompTargetEffect_Resurrect>() ||
            !TredecimUtils.HasFactorAndTredecim(corpse.InnerPawn))
        {
            return;
        }
        __result = false;
        if (showMessages) TredecimUtils.RejectResurrection(corpse.InnerPawn, nameof(Patch_CompTargetable_ValidateTarget));
    }
}

// Patch: 复活血清 Job 执行复活
// 跳过原方法时不销毁血清
[HarmonyPatch(typeof(JobDriver_Resurrect), "Resurrect")]
public static class Patch_JobDriver_Resurrect_Manual
{
    public static bool Prefix(JobDriver_Resurrect __instance)
    {
        if (__instance.job.GetTarget(TargetIndex.A).Thing is not Corpse corpse ||
            !TredecimUtils.HasFactorAndTredecim(corpse.InnerPawn))
        {
            return true;
        }
        TredecimUtils.RejectResurrection(corpse.InnerPawn, nameof(Patch_JobDriver_Resurrect_Manual));
        return false;
    }
}

// Patch: 复活能力选择目标
[HarmonyPatch(typeof(CompAbilityEffect_Resurrect), nameof(CompAbilityEffect_Resurrect.Valid),
    typeof(LocalTargetInfo), typeof(bool))]
public static class Patch_CompAbilityEffect_Resurrect_Valid
{
    public static void Postfix(LocalTargetInfo target, bool throwMessages, ref bool __result)
    {
        if (!__result || target.Thing is not Corpse corpse || !TredecimUtils.HasFactorAndTredecim(corpse.InnerPawn))
        {
            return;
        }
        __result = false;
        if (throwMessages) TredecimUtils.RejectResurrection(corpse.InnerPawn, nameof(Patch_CompAbilityEffect_Resurrect_Valid));
    }
}

// Patch: 复活能力施放
// 在 Ability.Activate 之前拒绝，不扣次数、冷却、心灵能量及其他效果组件的资源
[HarmonyPatch(typeof(Verb_CastAbility), "TryCastShot")]
public static class Patch_Verb_CastAbility_TryCastShot
{
    public static bool Prefix(Verb_CastAbility __instance, ref bool __result)
    {
        if (__instance.CurrentTarget.Thing is not Corpse corpse ||
            !__instance.ability.EffectComps.Any(c => c is CompAbilityEffect_Resurrect) ||
            !TredecimUtils.HasFactorAndTredecim(corpse.InnerPawn))
        {
            return true;
        }
        __result = false;
        return false;
    }
}

// Patch: 拒绝死亡的手动使用及 AI 预热完成
// 在扣除次数和开始复活计时之前拒绝
[HarmonyPatch(typeof(Hediff_DeathRefusal), "Use")]
public static class Patch_Hediff_DeathRefusal_Use
{
    public static bool Prefix(Hediff_DeathRefusal __instance)
    {
        if (!TredecimUtils.HasFactorAndTredecim(__instance.pawn)) return true;
        if (__instance.PlayerControlled) TredecimUtils.RejectResurrection(__instance.pawn, nameof(Patch_Hediff_DeathRefusal_Use));
        return false;
    }
}

// Patch: 死灵尘、死亡阴霾的蹒跚怪转化资格
// 在转化、修复部位和改变阵营之前排除尸体
[HarmonyPatch(typeof(MutantUtility), nameof(MutantUtility.CanResurrectAsShambler))]
public static class Patch_MutantUtility_CanResurrectAsShambler
{
    public static void Postfix(Corpse corpse, ref bool __result)
    {
        if (__result && TredecimUtils.HasFactorAndTredecim(corpse.InnerPawn)) __result = false;
    }
}
