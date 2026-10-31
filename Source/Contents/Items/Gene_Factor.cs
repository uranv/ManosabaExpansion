using UranvManosaba.Contents.Utils;
using Verse;

namespace UranvManosaba.Contents.Items;

public class Gene_Factor : Gene
{
    // 基因被添加时初始化
    private bool _initialized = false;
    public override void PostAdd()
    {
        base.PostAdd();
        EnsureDummyExists();
    }

    // 检查初始化完成
    public override void Tick()
    {
        base.Tick();
        if (!_initialized && pawn.IsHashIntervalTick(600))
        {
            EnsureDummyExists();
        }
    }

    private void EnsureDummyExists()
    {
        // 无地图时推迟初始化
        if (pawn?.Map == null || pawn?.health?.hediffSet == null) return;
        _initialized = true;
        // 不符合条件不再次初始化
        if (pawn.Dead || YukiGeneralUtils.IsYuki(pawn)) return;
        // 只有魔女残骸变种人才使用 MutantDummy，不能污染其他 mutant。
        if (pawn.mutant?.Def == ModDefOf.UmMutantNarehate)
        {
            if (!pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffMutantDummy))
            {
                pawn.health.AddHediff(ModDefOf.UmHediffMutantDummy);
            }
        }
        // 维护人类 dummy Hediff
        else if (!pawn.IsMutant && !pawn.health.hediffSet.HasHediff(ModDefOf.UmHediffHumanDummy))
        {
            var h = pawn.health.AddHediff(ModDefOf.UmHediffHumanDummy);
            Comps.HediffComp_HumanDummy.SetDummyShouldDisplay(pawn);
        }
    }
}