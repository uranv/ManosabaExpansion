using RimWorld;
using UranvManosaba.Contents.Comps;

namespace UranvManosaba.Contents.Items;

// 定义自定义攻击动词
public class Verb_BreakOnAttack : Verb_MeleeAttackDamage
{
    protected override bool TryCastShot()
    {
        bool result = base.TryCastShot();

        // 攻击成功时分解
        if (result)
        {
            if (CasterPawn != null && EquipmentSource != null)
            {
                var breakComp = EquipmentSource.GetComp<Comp_BreakOnAttack>();
                breakComp?.BreakAndDrop(CasterPawn);
            }
        }

        return result;
    }
}