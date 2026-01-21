using System.Reflection;
using RimWorld;
using Verse;
using Verse.Grammar;

namespace UranvManosaba.Contents.Utils;

/// <summary>
/// 文本生成静态类：
/// 生成无故事/殖民地故事文本
/// </summary>
public static class GrammarUtils
{
    private static FieldInfo _ruleStringOutputField;
    // 获取 Rule_String 类的私有字段 "output"
    static GrammarUtils()
    {
        _ruleStringOutputField = typeof(Rule_String).GetField("output", BindingFlags.NonPublic | BindingFlags.Instance);
    }


    // 保底无 Tale 的生成文本
    public static TaggedString GenerateStory(Pawn visitor, Pawn negitiator = null)
    {
        TaggedString generatedStory;
        TaggedString rootStory = "Manosaba_Story_Root".Translate(visitor, negitiator);
        var storyPack = ModDefOf.UmRulePackStory;
        var utilsPack = ModDefOf.UmRulePackUtils;
            
        var request = new GrammarRequest();
        request.Includes.Add(storyPack);
        request.Includes.Add(utilsPack);
        // 引用原版规则
        request.Includes.Add(RulePackDefOf.GlobalUtility);
        request.Includes.Add(RulePackDefOf.ArtDescriptionUtility_Global);

        generatedStory = GrammarResolver.Resolve("root", request);
        return rootStory+generatedStory;
    }
    // 生成 Tale 相关文本
    public static TaggedString GenerateTale(Pawn visitor, Pawn negitiator = null)
    {
        if (ManosabaMod.Settings.disableTale) return "ERR: manually disabled generated from tales.";
            
        var fallbackStory = GenerateStory(visitor, negitiator);
            
        // 尝试使用存档 Tale 生成
        var rootTale = "Manosaba_Tale_Root".Translate(visitor, negitiator);
        var validTales = Find.TaleManager
            .AllTalesListForReading.Where(t => t.def.usableForArt).ToList();

        if (!validTales.Any())
        {
            if (ManosabaMod.Settings.debugMode) Log.Warning("[Manosaba] no available tales for generation (Utils.GrammarUtils.GenerateTale)");
            return fallbackStory;
        }

        for (int i = 0; i < 10; i++)
        {
            var candidateTale = validTales.RandomElement();
            try
            {
                if (!IsTaleSafe(candidateTale)) 
                {
                    if(ManosabaMod.Settings.debugMode) 
                        Log.Warning($"[Manosaba] Skipped unsafe tale: {candidateTale.def.defName}, ID:{candidateTale.id} (Utils.GrammarUtils.GenerateTale)");
                    continue; 
                }
                var request = new GrammarRequest();
                request.Includes.Add(ModDefOf.UmRulePackTale);
                // 引用原版规则
                request.Includes.Add(RulePackDefOf.GlobalUtility);
                request.Includes.Add(RulePackDefOf.ArtDescriptionUtility_Global);
                request.Includes.Add(RulePackDefOf.ArtDescriptionRoot_HasTale);
                // 获取原版规则
                request.IncludesBare.AddRange(candidateTale.GetTextGenerationIncludes());
                List<Rule> vanillaRules = candidateTale.GetTextGenerationRules(request.Constants).ToList();
                // 替换 nameFull 为 nameDef
                ReplaceNameFullWithDef(vanillaRules);
                request.Rules.AddRange(vanillaRules);
                // 注入自定义代词: PAWN1, PAWN2
                AddGenericPawnAliases(ref request, candidateTale);
                // 生成文本
                return rootTale + GrammarResolver.Resolve("root", request, candidateTale.def.defName);
            }
            catch (Exception ex)
            {
                var text =
                    $"[Manosaba] Failed to analysis tale {candidateTale.id}: {ex.Message}. ";
                var text2 = i == 9 ? 
                    "Falling back to standard story (Utils.GrammarUtils.GenerateTale)" : 
                    $"Next try after {i+1}th time(Utils.GrammarUtils.GenerateTale)";
                Log.Warning(text + text2);
            }
        }
            
        return fallbackStory;
    }

    // 为 Tale 注入通用 Pawn 代词规则
    private static void AddGenericPawnAliases(ref GrammarRequest request, Tale tale)
    {
        Pawn pawn1 = null;
        Pawn pawn2 = null;
        switch (tale)
        {
            // 故事分类
            case Tale_DoublePawn doublePawn:
                pawn1 = doublePawn.firstPawnData?.pawn;
                pawn2 = doublePawn.secondPawnData?.pawn;
                break;
            case Tale_SinglePawn singlePawn:
                pawn1 = singlePawn.pawnData?.pawn;
                break;
            //case Tale_DoublePawnAndDef doublePawnAndDef:
            //pawn1 = doublePawnAndDef.firstPawnData?.pawn;
            //pawn2 = doublePawnAndDef.secondPawnData?.pawn;
            //break;
            //case Tale_DoublePawnAndTrader doublePawnAndTrader:
            //pawn1 = doublePawnAndTrader.firstPawnData?.pawn;
            //pawn2 = doublePawnAndTrader.secondPawnData?.pawn;
            //break;
            //case Tale_DoublePawnKilledBy doublePawnKilledBy:
            //pawn1 = doublePawnKilledBy.firstPawnData?.pawn;
            //pawn2 = doublePawnKilledBy.secondPawnData?.pawn;
            //break;
            //case Tale_SinglePawnAndDef singlePawnAndDef:
            //pawn1 = singlePawnAndDef.pawnData?.pawn;
            //break;
            //case Tale_SinglePawnAndThing singlePawnAndThing:
            //pawn1 = singlePawnAndThing.pawnData?.pawn;
            //break;
            //case Tale_SinglePawnAndTrader singlePawnAndTrader:
            //pawn1 = singlePawnAndTrader.pawnData?.pawn;
            //break;
        }
        // 注入参数
        if (pawn1 != null)
        {
            request.Rules.AddRange(GrammarUtility.RulesForPawn("PAWN1", pawn1, request.Constants));
        }
        if (pawn2 != null)
        {
            request.Rules.AddRange(GrammarUtility.RulesForPawn("PAWN2", pawn2, request.Constants));
            request.Constants["HasSecondPawn"] = "True";
        }
        else
        {
            request.Constants["HasSecondPawn"] = "False";
            request.Rules.Add(new Rule_String("PAWN2_nameDef", "没有人"));
        }
    }

    // 替换 RulePack 中的 nameFull 内容为 nameDef 内容
    private static void ReplaceNameFullWithDef(List<Rule> rules)
    {
        if (_ruleStringOutputField == null)
        {
            if(ManosabaMod.Settings.debugMode) Log.Error("[Manosaba] Failed to get reflection info for Rule_String.output (Utils.GrammarUtils.ReplaceNameFullWithDef)");
            return;
        }

        Dictionary<string, string> nameDefMap = new Dictionary<string, string>();

        // 读取阶段
        foreach (var rule in rules)
        {
            if (rule is Rule_String rStr && rStr.keyword.EndsWith("_nameDef"))
            {
                string prefix = rStr.keyword.Substring(0, rStr.keyword.Length - 8);
                nameDefMap[prefix] = rStr.Generate();
            }
        }

        // 写入阶段
        foreach (var rule in rules)
        {
            if (rule is Rule_String rStr && rStr.keyword.EndsWith("_nameFull"))
            {
                string prefix = rStr.keyword.Substring(0, rStr.keyword.Length - 9);

                if (nameDefMap.TryGetValue(prefix, out string shortNameContent))
                {
                    // 【核心修改】使用 FieldInfo.SetValue 强行修改私有字段
                    _ruleStringOutputField.SetValue(rStr, shortNameContent);
                }
            }
        }
    }
    // 快速检查选取 tale 安全性
    private static bool IsTaleSafe(Tale tale)
    {
        if (tale == null) return false;
            
        Pawn p1 = null;
        Pawn p2 = null;

        if (tale is Tale_DoublePawn doublePawn)
        {
            p1 = doublePawn.firstPawnData?.pawn;
            p2 = doublePawn.secondPawnData?.pawn;
        }
        else if (tale is Tale_SinglePawn singlePawn)
        {
            p1 = singlePawn.pawnData?.pawn;
        }
        if (p1 != null && !IsPawnSafeForGrammar(p1)) return false;
        if (p2 != null && !IsPawnSafeForGrammar(p2)) return false;

        return true;
    }
    private static bool IsPawnSafeForGrammar(Pawn p)
    {
        if (p == null) return false;
        if (p.Name == null || p.kindDef == null) return false;
        if (p.ageTracker == null) return false;
        if (p.relations == null) return false;
        return true;
    }

}