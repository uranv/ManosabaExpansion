using HarmonyLib;
using RimWorld;
using UranvManosaba.Contents.PsychicRituals;
using Verse;

namespace UranvManosaba.Contents.Patches;

// Patch: 修改 PsychicRitualRoleDef.PawnCanDo
// 二次筛选仪式参与者和自定义原因
[HarmonyPatch(typeof(PsychicRitualRoleDef), "PawnCanDo")]
[HarmonyPatch(new[] { typeof(PsychicRitualRoleDef.Context), typeof(Pawn), typeof(TargetInfo), typeof(PsychicRitualRoleDef.Reason) }, new[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out })]
public static class Patch_PsychicRitualRoleDef_PawnCanDo_Public
{
    public static void Postfix(PsychicRitualRoleDef __instance, 
        PsychicRitualRoleDef.Context context, 
        Pawn pawn, 
        TargetInfo target, 
        ref PsychicRitualRoleDef.Reason reason,
        ref bool __result)
    {
        if (__instance.defName != "UmRitualRoleNarehateTransTarget" && __instance.defName != "UmRitualRoleSabbatInvoker" && __instance.defName != "UmRitualRoleSabbatChanter") return;
        if (!__result) return;
        switch (__instance.defName)
        {
            // 魔女残骸洗脑仪式: 仪式对象必须是【魔女残骸】
            case "UmRitualRoleNarehateTransTarget":
            {
                bool isNarehateMutant = pawn.IsMutant && pawn.mutant.Def.defName == "UmMutantNarehate";
                bool isNarehateColonist = pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffNarehate);
                if (!isNarehateMutant && !isNarehateColonist)
                {
                    __result = false;
                    reason = new PsychicRitualRoleDef.Reason(
                        __instance,
                        context,
                        pawn,
                        target,
                        AnyEnum.FromEnum(ManosabaRitualCondition.NotNarehate)
                    );
                }
                return;
            }
            // 魔女安息仪式: 主持人必须拥有【魔女因子】(且持有仪式剑)
            case "UmRitualRoleSabbatInvoker":
            {
                bool hasDummyHediff = pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffHumanDummy);
                //bool hasRitualSword = pawn.equipment?.Primary?.def == Anomaly.ModDefOf.UmThingWeaponRitualSword;
                //hasRitualSword = (ritualSword != null) ? pawn.equipment?.Primary?.def == ritualSword : false;
                if (!hasDummyHediff)
                {
                    __result = false;
                    reason = new PsychicRitualRoleDef.Reason(
                        __instance,
                        context,
                        pawn,
                        target,
                        AnyEnum.FromEnum(ManosabaRitualCondition.NoMajyoinshi)
                    );
                }
                //else if (!hasRitualSword)
                //{
                //    __result = false;
                //    reason = new PsychicRitualRoleDef.Reason(
                //        __instance, // def
                //        context, // context
                //        pawn, // pawn
                //        target, // target
                //        AnyEnum.FromEnum(Anomaly.ManosabaRitualCondition.NoRitualSword) // reasonCode
                //    );
                //}
                break;
            }
            // 魔女安息仪式: 参与者必须拥有【魔女因子】
            case "UmRitualRoleSabbatChanter":
            {
                bool hasDummyHediff = pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffHumanDummy);
                if (!hasDummyHediff)
                {
                    __result = false;
                    reason = new PsychicRitualRoleDef.Reason(
                        __instance,
                        context,
                        pawn,
                        target,
                        AnyEnum.FromEnum(ManosabaRitualCondition.NoMajyoinshi)
                    );
                }
                break;
            }
        }
    }
}