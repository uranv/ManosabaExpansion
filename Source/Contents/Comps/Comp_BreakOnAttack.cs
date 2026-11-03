using RimWorld;
using Verse;
using Verse.Sound;

namespace UranvManosaba.Contents.Comps;

public class CompProperties_BreakOnAttack : CompProperties
{
    public ThingDef DropSwordDef;
    public ThingDef DropWoodDef;
    public ThingDef DropClothDef;
    public CompProperties_BreakOnAttack()
    {
        compClass = typeof(Comp_BreakOnAttack);
    }
}
public class Comp_BreakOnAttack : ThingComp
{
    private CompProperties_BreakOnAttack Props => (CompProperties_BreakOnAttack)props;

    public void BreakAndDrop(Pawn bearer)
    {
        // 生成掉落物
        if (Props.DropSwordDef != null)
        {
            var droppedItem = ThingMaker.MakeThing(Props.DropSwordDef, ThingDefOf.Steel);
            droppedItem.stackCount = 1;
            GenSpawn.Spawn(droppedItem, bearer.Position, bearer.Map);
        }
        if (Props.DropWoodDef != null)
        {
            var droppedItem = ThingMaker.MakeThing(Props.DropWoodDef);
            droppedItem.stackCount = 1;
            GenSpawn.Spawn(droppedItem, bearer.Position, bearer.Map);
        }
        if (Props.DropClothDef != null)
        {
            var droppedItem = ThingMaker.MakeThing(Props.DropClothDef);
            droppedItem.stackCount = 2;
            GenSpawn.Spawn(droppedItem, bearer.Position, bearer.Map);
        }
        // 播放一个破碎的声音或特效
        SoundDefOf.MetalHitImportant.PlayOneShot(bearer);

        // 销毁自身
        if (parent.stackCount > 1)
        {
            parent.SplitOff(1).Destroy();
        }
        else
        {
            parent.Destroy();
        }

        // 发送提示
        //Messages.Message("MessageWeaponBroke".Translate(parent.Label), bearer, MessageTypeDefOf.NeutralEvent, false);
    }
}

