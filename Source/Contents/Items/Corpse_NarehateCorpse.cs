using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Items;

public class Corpse_NarehateCorpse : Corpse
{
    public override string Label
    {
        get
        {
            string label = "Manosaba_NarehateCorpse_Label".Translate();
            return label; 
        }
    }

    
    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (respawningAfterLoad) return;
        
        // 解锁图鉴: 魔女残骸的尸体
        var codexDef1 = DefDatabase<EntityCodexEntryDef>.GetNamed("UmEntryCorpse", false);
        if (codexDef1 is { Discovered: false })
        {
            Find.EntityCodex.SetDiscovered(codexDef1, def);
        }
        // 解锁图鉴: 魔女残骸
        var codexDef2 = DefDatabase<EntityCodexEntryDef>.GetNamed("UmEntryMutant", false);
        if (codexDef2 is { Discovered: false })
        {
            Find.EntityCodex.SetDiscovered(codexDef2, def);
        }
    }
    
    public override void Notify_Studied(Pawn studier, float amount, KnowledgeCategoryDef category = null)
    {
        const float maxKnowledge = 10.0f;  
            
        var studiable = GetComp<CompStudiable>();
        if (studiable == null) 
        {
            base.Notify_Studied(studier, amount, category);
            return;
        }

        var currentProgress = studiable.ProgressPercent;

        studiable.anomalyKnowledgeGained = currentProgress * maxKnowledge;

        base.Notify_Studied(studier, amount, category);  

        if (currentProgress >= 1.0f)
        {
            var comp = Current.Game.GetComponent<ManosabaGameComponent>();
            if (comp is { isAburiBinUnlocked: false })
            {
                string logNotification = "RecipeAburiBinUnlocked_Log".Translate();
                Log.Error(logNotification);
                comp.isAburiBinUnlocked = true;
                Find.LetterStack.ReceiveLetter(
                    "RecipeAburiBinUnlocked_Label".Translate(), //"解锁制作: 炙烤药瓶", 
                    "RecipeAburiBinUnlocked_Text".Translate(),//"魔女原来只靠一瓶药就能杀死吗……\n\n看着这个药瓶, 你似乎又有了一些其他想法。", 
                    LetterDefOf.PositiveEvent, 
                    this
                );
                // 将当前的工作点数直接设置为 XML 中定义的完工总量
                if (studiable.Props.Completable)
                {
                    studiable.studyPoints = studiable.Props.studyAmountToComplete;
                }
            }
        }
    }
        
}