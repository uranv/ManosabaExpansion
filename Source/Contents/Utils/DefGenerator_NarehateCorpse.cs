using HarmonyLib;
using RimWorld;
using UnityEngine;
using UranvManosaba.Contents.Items;
using Verse;

namespace UranvManosaba.Contents.Utils;

[StaticConstructorOnStartup]
public static class DefGenerator_NarehateCorpse
{
    static DefGenerator_NarehateCorpse()
    {
        CorpseDef_NarehateCorpse();
    }

    private static void CorpseDef_NarehateCorpse()
    {
        // 获取人类及其原版尸体 Def
        var humanDef = ThingDefOf.Human;
        if (humanDef == null) return;
        var humanCorpseDef = humanDef.race.corpseDef;

        // 创建新 Def
        var narehateDef = new ThingDef
        {
            defName = "UmThingCorpseNarehate",
            label = "ThingDef_NarehateCorpse_label".Translate(),
            description = "ThingDef_NarehateCorpse_description".Translate(),
            uiIconPath = "ui/icons/menu_icon_narehate_corpse"
        };

        narehateDef.uiIcon = ContentFinder<Texture2D>.Get(narehateDef.uiIconPath);  // 手动唤起加载资源
        if (narehateDef.uiIcon == null && ManosabaMod.Settings.debugMode)
        {
            Log.Warning("[Manosaba] UI Icon load failed: " + narehateDef.uiIconPath + " (Utils.DefGenerator_NarehateCorpse.CorpseDef_NarehateCorpse)");
        }
        
        narehateDef.thingClass = typeof(Corpse_NarehateCorpse); // 绑定到自定义尸体类
        narehateDef.race = humanDef.race; // 借用人类种族数据, 防止 StatWorker 崩溃
        narehateDef.category = ThingCategory.Item;
        narehateDef.tickerType = TickerType.Rare;
        narehateDef.altitudeLayer = AltitudeLayer.ItemImportant;
        narehateDef.selectable = true;
        narehateDef.alwaysHaulable = true;
        narehateDef.soundDrop = SoundDefOf.Corpse_Drop;
        narehateDef.pathCost = 14;
        narehateDef.useHitPoints = true;
        narehateDef.SetStatBaseValue(StatDefOf.MaxHitPoints, humanDef.BaseMaxHitPoints);
        narehateDef.SetStatBaseValue(StatDefOf.Flammability, 1.0f);
        narehateDef.SetStatBaseValue(StatDefOf.DeteriorationRate, 2.0f);
        narehateDef.SetStatBaseValue(StatDefOf.Mass, 60f);

        // 复制人类尸体面板
        if (humanCorpseDef.inspectorTabs != null)
        {
            narehateDef.inspectorTabs = new List<Type>(humanCorpseDef.inspectorTabs);
        }
        // 添加异象调查面板
        if (!narehateDef.inspectorTabs.Contains(typeof(ITab_StudyNotes)))
        {
            narehateDef.inspectorTabs.Add(typeof(ITab_StudyNotes));
        }
            
        // 复制分类
        narehateDef.thingCategories = new List<ThingCategoryDef> { ThingCategoryDefOf.CorpsesHumanlike };

        // 添加comps
        narehateDef.comps = new List<CompProperties>
        {
            // 复制腐烂组件
            // narehateDef.comps.Add(new CompProperties_Rottable { daysToRotStart = 2.5f, rotDestroys = true });
            new CompProperties_Forbiddable()
        };

        // 异象研究comp
        var studyComp = new CompProperties_Studiable
        {
            frequencyTicks = 60000,
            minMonolithLevelForStudy = 1,
            showToggleGizmo = true,
            studyAmountToComplete = 20f
        };
        // 设置单次研究点数
        AccessTools.Field(typeof(CompProperties_Studiable), "anomalyKnowledge")
            ?.SetValue(studyComp, 2.0f);
        // 设置研究分类
        AccessTools.Field(typeof(CompProperties_Studiable), "knowledgeCategory")
            ?.SetValue(studyComp, KnowledgeCategoryDefOf.Basic);
        // 设置异象研究等级
        AccessTools.Field(typeof(CompProperties_Studiable), "minMonolithLevelForStudy")
            ?.SetValue(studyComp,1); 


        narehateDef.comps.Add(studyComp);
        
        // 研究日志comp
        var unlocksComp = new CompProperties_StudyUnlocks
        {
            studyNotes = new List<StudyNote>()
        };
        var note1 = new StudyNote
        {
            threshold = 8.0f,
            label = "ThingDef_NarehateCorpse_Note1_label".Translate(), //"初步解析";
            text = "ThingDef_NarehateCorpse_Note1_text".Translate() //"经过初步调查, 我们发现这具魔女残骸的死亡是通过......";
        };
        unlocksComp.studyNotes.Add(note1);
        var note2 = new StudyNote
        {
            threshold = 20.0f,
            label = "ThingDef_NarehateCorpse_Note2_label".Translate(), //"解析完成";
            text = "ThingDef_NarehateCorpse_Note2_text".Translate() //"我们已经完全解析了这具魔女残骸尸体的秘密。......";
        };
        unlocksComp.studyNotes.Add(note2);
        narehateDef.comps.Add(unlocksComp);


        // Ingestible 配置
        narehateDef.ingestible = new IngestibleProperties
        {
            parent = narehateDef,
            foodType = FoodTypeFlags.Corpse,
            sourceDef = humanDef,
            preferability = FoodPreferability.DesperateOnly
        };

        // 注册到数据库
        DefDatabase<ThingDef>.Add(narehateDef);
        narehateDef.ResolveReferences();
        if (ManosabaMod.Settings.debugMode) Log.Message("[Manosaba] <ThingDef> UmThingCorpseNarehate created (Utils.DefGenerator_NarehateCorpse.CorpseDef_NarehateCorpse)");

        // 添加到储存列表
        foreach (var category in narehateDef.thingCategories)
        {
            if (!category.childThingDefs.Contains(narehateDef))
            {
                category.childThingDefs.Add(narehateDef);
            }
        }
        // 关联到异象图鉴
        var codexEntry = DefDatabase<EntityCodexEntryDef>.GetNamed("UmEntryCorpse", false);
        if (codexEntry != null)
        {
            codexEntry.linkedThings ??= new List<ThingDef>();
            if (!codexEntry.linkedThings.Contains(narehateDef))
            {
                codexEntry.linkedThings.Add(narehateDef);
                if (ManosabaMod.Settings.debugMode) Log.Message("[Manosaba] <ThingDef> UmThingCorpseNarehate linked to <EntityCodexEntryDef> UmEntryCorpse (Utils.DefGenerator_NarehateCorpse.CorpseDef_NarehateCorpse)");
            }
        }
        else
        {
            Log.Error("[Manosaba] <EntityCodexEntryDef> UmEntryCorpse not found (Defs/EntityCodexEntryDefs/CodexEntries.xml)");
        }

        // 关联到工作点配方 -- 遍历并添加到所有允许人类尸体的配方
        foreach (var recipe in DefDatabase<RecipeDef>.AllDefs)
        {
            if (recipe.fixedIngredientFilter == null ||
                !recipe.fixedIngredientFilter.AllowedThingDefs.Contains(humanCorpseDef)) continue;
            
            recipe.fixedIngredientFilter.SetAllow(narehateDef, true);

            // 更新默认过滤器
            if (recipe.defaultIngredientFilter != null)
            {
                var allowHumanCorpse = recipe.defaultIngredientFilter.AllowedThingDefs.Contains(humanCorpseDef);
                recipe.defaultIngredientFilter.SetAllow(narehateDef, allowHumanCorpse);
            }

            // 更新配方成分
            if (recipe.ingredients == null) continue;
            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredient.filter != null && 
                    ingredient.filter.AllowedThingDefs.Contains(humanCorpseDef))
                {
                    ingredient.filter.SetAllow(narehateDef, true);
                    if (ManosabaMod.Settings.debugMode) Log.Message($"[Manosaba] <ThingDef> UmThingCorpseNarehate linked to <RecipeDef> {recipe} (Utils.DefGenerator_NarehateCorpse.CorpseDef_NarehateCorpse)");
                }
            }
        }
    }
}