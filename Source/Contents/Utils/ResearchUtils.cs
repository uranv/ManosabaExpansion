using HarmonyLib;
using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Utils;

public static class ResearchUtils
{
    // 解锁前置科技，系列行为
    public static void UnlockResearchPrereqs()
    {
        if (!ModDefOf.UmResearchProjectPrereqs.IsFinished)
        {
            Find.ResearchManager.FinishProject(ModDefOf.UmResearchProjectPrereqs, doCompletionDialog: false, researcher: null);
            Find.LetterStack.ReceiveLetter(
                "Manosaba_ResearchUnlocked_Label".Translate(),
                "Manosaba_ResearchUnlocked_Text".Translate(),
                LetterDefOf.PositiveEvent
            );
        }
        // 发现魔女因子时, 解锁图鉴条目
        var codexDef = DefDatabase<EntityCodexEntryDef>.GetNamed("UmEntryFactor", false);
        if (codexDef is { Discovered: false }) UnlockCodexEntrySilent("UmEntryFactor");
        // 解简易长矛配方
        var comp = Current.Game.GetComponent<ManosabaGameComponent>();
        if (comp is { isSimpleSpearUnlocked: false })
        {
            string logNotification = "RecipeSimpleSpearUnlocked_Log".Translate();
            Log.Error(logNotification);
            comp.isSimpleSpearUnlocked = true;
            Find.LetterStack.ReceiveLetter(
                "RecipeSimpleSpearUnlocked_Label".Translate(), //"解锁制作: 简易长矛", 
                "RecipeSimpleSpearUnlocked_Text".Translate(),//"你现在可以制作简易长矛了。", 
                LetterDefOf.PositiveEvent
            );
        }
    }
    // 静默解锁异象图鉴条目
    private static void UnlockCodexEntrySilent(string entryDefName, ThingDef discoveredDef = null)
    {
        var codexDef = DefDatabase<EntityCodexEntryDef>.GetNamed(entryDefName, false);

        if (codexDef is { Discovered: false })
        {
            var codex = Find.EntityCodex;

            // 获取私有字段引用
            var hiddenEntriesDict = AccessTools.Field(typeof(EntityCodex), "hiddenEntries")
                .GetValue(codex) as Dictionary<EntityCodexEntryDef, bool>;
                    
            var hiddenCategoriesDict = AccessTools.Field(typeof(EntityCodex), "hiddenCategories")
                .GetValue(codex) as Dictionary<EntityCategoryDef, bool>;
                
            var discoveredEntitiesSet = AccessTools.Field(typeof(EntityCodex), "discoveredEntities")
                .GetValue(codex) as HashSet<ThingDef>;
                
            // 解锁条目
            if (hiddenEntriesDict != null) hiddenEntriesDict[codexDef] = false;
            // 解锁分类 (防止分类本身还未显示)
            if (hiddenCategoriesDict != null && codexDef.category != null) hiddenCategoriesDict[codexDef.category] = false;
            // 标记关联物体为已发现
            if (discoveredDef != null && discoveredEntitiesSet != null && entryDefName != null)
            {
                discoveredEntitiesSet.Add(discoveredDef);
                Find.HiddenItemsManager.SetDiscovered(discoveredDef);
            }

            // 发送信号
            // 虽然不是必须的, 但某些任务或脚本可能会监听这个信号
            // 由于我们跳过了 SetDiscovered, 如果想保持最大兼容性, 可以手动发一下（不会触发信件）
            Find.SignalManager.SendSignal(new Signal(EntityCodex.EntityDiscoveredSignal, global: true));
                
            // 调试日志
            if (ManosabaMod.Settings.debugMode) Log.Message($"[Manosaba] Unlock <EntityCodexEntryDef> {codexDef.defName} silently (Utils.ResearchUtils.UnlockCodexEntrySilent)");
        }
    }



}