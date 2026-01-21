using RimWorld;
using UranvManosaba.Contents.Items;
using Verse;

namespace UranvManosaba.Contents.Utils;

public static class NarehateCorpseGenerator
{
    public static Corpse_NarehateCorpse GenCorpse()
    {
        // 生成人类 Pawn
        var request = new PawnGenerationRequest(
            PawnKindDefOf.Colonist, Faction.OfAncients, PawnGenerationContext.NonPlayer, -1,
            forceGenerateNewPawn: true, fixedBiologicalAge: 15, allowDead: false, allowDowned: false,
            fixedGender: Gender.Female, forceNoGear: true);
        
        var pawn = PawnGenerator.GeneratePawn(request);
            
        // 添加 Hediff
        pawn.health.AddHediff(ModDefOf.UmHediffNarehate);
        pawn.health.AddHediff(ModDefOf.UmHediffTredecim);
            
        var hediff = HediffMaker.MakeHediff(ModDefOf.UmHediffHumanDummy, pawn);
        hediff.Severity = 1f;
        var compProgress = hediff.TryGetComp<Comps.HediffComp_Progress>();
        if (compProgress != null)
        {
            compProgress.isFinished = true;
            compProgress.isDisplay = false;
        }
        pawn.health.AddHediff(hediff);

        // 添加绘制comp
        if (pawn.GetComp<Comps.Comp_NarehateVisuals>() == null)
        {
            var comp = new Comps.Comp_NarehateVisuals { parent = pawn };
            pawn.AllComps.Add(comp);
        }

        // 杀死为自定义尸体
        pawn.Kill(null, null);
        var standardCorpse = pawn.Corpse;
        if (standardCorpse == null) 
        {
            Log.Error("[Manosaba] Narehate corpse generate failed: standardCorpse == null (Utils.NarehateCorpseGenerator.GenCorpse)");
            return null;
        }

        // 创建定义 NarehateCorpse
        var myDef = DefDatabase<ThingDef>.GetNamed("UmThingCorpseNarehate");
        var specialCorpse = (Corpse_NarehateCorpse)ThingMaker.MakeThing(myDef);

        // 将 Pawn 从原尸体转移到新尸体
        var oldOwner = standardCorpse.GetDirectlyHeldThings();
        var newOwner = specialCorpse.GetDirectlyHeldThings();
        oldOwner.TryTransferAllToContainer(newOwner);
        standardCorpse.Destroy();

        return specialCorpse;
    }
}