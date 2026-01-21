using RimWorld;
using RimWorld.QuestGen;
using UranvManosaba.Contents.Utils;
using Verse;
using Verse.Grammar;

namespace UranvManosaba.Contents.Incidents;

public class QuestNode_Root_MystCargoNarehate : QuestNode_Root_MysteriousCargo
{
    public string inSignal;
    protected override bool RequiresPawn => false;
    
    protected override void RunInt()
    {
        var vanillaDef = DefDatabase<QuestScriptDef>.GetNamed("MysteriousCargoUnnaturalCorpse", false);
        if (vanillaDef != null)
        {
            QuestGen.QuestDescriptionRulesReadOnly.Clear();
            QuestGen.QuestNameRulesReadOnly.Clear();

            if (vanillaDef.questDescriptionRules != null)
                QuestGen.AddQuestDescriptionRules(vanillaDef.questDescriptionRules);
            if (vanillaDef.questNameRules != null)
                QuestGen.AddQuestNameRules(vanillaDef.questNameRules);
            if (vanillaDef.questDescriptionAndNameRules != null)
            {
                QuestGen.AddQuestDescriptionRules(vanillaDef.questDescriptionAndNameRules);
                QuestGen.AddQuestNameRules(vanillaDef.questDescriptionAndNameRules);
            }
        }
        base.RunInt();

        // 补充缺失翻译
        var asker = QuestGen.slate.Get<Pawn>("asker");

        if (asker == null) return;
        // 使用 GrammarUtility.RulesForPawn 生成标准的人称规则 (asker_nameFull, asker_pronoun 等)
        QuestGen.AddQuestDescriptionRules(GrammarUtility.RulesForPawn("asker", asker).ToList());
        
        if (asker.Faction == null) return;
        var extraRules = new List<Rule>
        {
            // 对应翻译文本中的 [asker_faction_name]
            new Rule_String("asker_faction_name", asker.Faction.Name),
            // 对应翻译文本中的 [asker_faction_leaderTitle]
            new Rule_String("asker_faction_leaderTitle", asker.Faction.LeaderTitle)
        };

        // 调用重载
        QuestGen.AddQuestDescriptionRules(extraRules);
    }

    // 生成尸体
    protected override Thing GenerateThing(Pawn _)
    {
        var corpse = NarehateCorpseGenerator.GenCorpse();

        // 解锁相关前置
        //Manosaba_Utils.UnlockResearchPrereqs();

        // 将名字注入 Slate 在 XML 中调用
        if (corpse.InnerPawn != null)
        {
            QuestGen.slate.Set("narehateName", corpse.InnerPawn.Name.ToStringShort);
            QuestGen.slate.Set("narehateLabel", corpse.InnerPawn.LabelShort);
        }
        return corpse;
    }

    // 添加 QuestParts
    protected override void AddPostDroppedQuestParts(Pawn pawn, Thing thing, Quest quest)
    {
        base.AddPostDroppedQuestParts(pawn, thing, quest);
        
        var unlockPart = new QuestPart_MystCargoNarehate_Unlock
        {
            // 将它的触发信号设置为当前上下文的信号 (即空投落地的信号)
            // 在基类逻辑中, 此时 QuestGen.slate 中的 "inSignal" 指向的是 Delay 结束后的信号
            inSignal = QuestGen.slate.Get<string>("inSignal"),
        };
        
        quest.AddPart(unlockPart);
    }
}