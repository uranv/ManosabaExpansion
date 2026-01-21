using RimWorld;
using Verse;
using Verse.Sound;

namespace UranvManosaba.Contents.Comps;

public class CompProperties_BreakOnAttack : CompProperties
{
    public ThingDef dropSwordDef;
    public ThingDef dropWoodDef;
    public ThingDef dropClothDef;
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
        if (Props.dropSwordDef != null)
        {
            var droppedItem = ThingMaker.MakeThing(Props.dropSwordDef, ThingDefOf.Steel);
            droppedItem.stackCount = 1;
            GenSpawn.Spawn(droppedItem, bearer.Position, bearer.Map);
        }
        if (Props.dropWoodDef != null)
        {
            var droppedItem = ThingMaker.MakeThing(Props.dropWoodDef);
            droppedItem.stackCount = 1;
            GenSpawn.Spawn(droppedItem, bearer.Position, bearer.Map);
        }
        if (Props.dropClothDef != null)
        {
            var droppedItem = ThingMaker.MakeThing(Props.dropClothDef);
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

