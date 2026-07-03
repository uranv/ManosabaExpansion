using RimWorld;
using Verse;

namespace UranvManosaba.Contents.PsychicRituals;

public enum ManosabaRitualCondition { NotNarehate, NoMajyoinshi, NoRitualSword }
public class PsychicRitualRoleDef_NarehateTarget : PsychicRitualRoleDef
{
    public override bool CanHandleOfferings => true;
    public override TaggedString PawnCannotDoReason(AnyEnum reason, Context context, Pawn pawn, TargetInfo target)
    {
        ManosabaRitualCondition? myReason = reason.As<ManosabaRitualCondition>();
        if (myReason is ManosabaRitualCondition.NotNarehate)
        {
            return "Manosaba_NarehateTargetInvalid".Translate(pawn.LabelShort);
        }
        return base.PawnCannotDoReason(reason, context, pawn, target);
    }
}
    
public class PsychicRitualRoleDef_SabbatInvoker : PsychicRitualRoleDef
{
    // 允许寻路获得献祭物
    public override bool CanHandleOfferings => true;
    public override TaggedString PawnCannotDoReason(AnyEnum reason, Context context, Pawn pawn, TargetInfo target)
    {
        ManosabaRitualCondition? myReason = reason.As<ManosabaRitualCondition>();
        if (myReason is ManosabaRitualCondition.NoMajyoinshi)
        {
            return "Manosaba_Sabbat_NoMajyoinshi".Translate(pawn.LabelShort);
        }
        //else if (myReason.HasValue && myReason.Value == ManosabaRitualCondition.NoRitualSword)
        //{
        //    return "Manosaba_Sabbat_NoRitualSword".Translate(pawn.LabelShort);
        //}
        return base.PawnCannotDoReason(reason, context, pawn, target);
    }
}

public class PsychicRitualRoleDef_SabbatChanter : PsychicRitualRoleDef
{
    // 不查找献祭物（默认）
    public override bool CanHandleOfferings => false;
    public override TaggedString PawnCannotDoReason(AnyEnum reason, Context context, Pawn pawn, TargetInfo target)
    {
        ManosabaRitualCondition? myReason = reason.As<ManosabaRitualCondition>();
        if (myReason is ManosabaRitualCondition.NoMajyoinshi)
        {
            return "Manosaba_Sabbat_NoMajyoinshi".Translate(pawn.LabelShort);
        }
        //else if (myReason.HasValue && myReason.Value == ManosabaRitualCondition.NoRitualSword)
        //{
        //    return "Manosaba_Sabbat_NoRitualSword".Translate(pawn.LabelShort);
        //}
        return base.PawnCannotDoReason(reason, context, pawn, target);
    }
}