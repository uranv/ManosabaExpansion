using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UranvManosaba.Contents;

public class ManosabaModSettings : ModSettings
{
    // 一般设置
    public bool isShowProgress = false;  // HediffComp_ShowSeverity & HediffComp_Progress
    public bool debugMode = false; 

    public float previewMood = 50;  // do not save
    public float previewMinorThres = 35;  // do not save
    public float previewSeverity = 50f;  // do not save


    // 魔女化设置页 
    public float severityIncreaseFactor = 16.0f;  // HediffComp_Progress
    public float severityDecreaseFactor = 8.0f;  // HediffComp_Progress
    public float severityIncreaseBias = 23.0f;  // HediffComp_Progress
    public float severityFlatBias = 8.0f;  // HediffComp_Progress
    public float severityIncreaseExponent = 1.0f;  // HediffComp_Progress
    public float severityDecreaseExponent = 1.0f;  // HediffComp_Progress
    public float baseMtbDays = 60;  // HediffComp_Progress



    // 魔女残骸治疗设置
    public float factorHealing = 500f;  // HediffComp_Narehate & HediffComp_Hanmajyo
    public float factorBloodHealing = 67f;  // HediffComp_Narehate & HediffComp_Hanmajyo
    public bool isBloodHealing = false;  // HediffComp_Narehate & HediffComp_Hanmajyo
    public float postFactorHealing = 0.22f;  // HediffComp_Narehate & HediffComp_Hanmajyo
    // 后处理系数作用于: 半魔女奖励, 小雪；使用tick为600, 即默认为 factorHealing / 10 * factorBloodHealing
    public bool isPostHealing = true;  // HediffComp_Narehate & HediffComp_Hanmajyo
        

    // 魔女变种人控制时间
    public float mutantFullCircle = 100.0f;  // HediffComp_Countdown_Mutant
    // 魔女安息仪式设置
    public float thresholdMinusOne = 25;  // PsychicRitualDef_Sabbat
    public float thresholdZero = 55;  // PsychicRitualDef_Sabbat
    public float thresholdPlusOne = 85;  // PsychicRitualDef_Sabbat


    // 其他设置
    public float inverseTemperature = 5f;  // Manosaba_Utils
    public bool allowMutantAssignTab;  // Patch_MainTabWindow_Assign_Pawns
    public bool disableTale = false;  // Grammar_Utils

        
    public override void ExposeData()
    {
        Scribe_Values.Look(ref debugMode, "debugMode", false);


        Scribe_Values.Look(ref isShowProgress, "isShowProgress", false);
        Scribe_Values.Look(ref severityIncreaseFactor, "severityIncreaseFactor", 16.0f);
        Scribe_Values.Look(ref severityDecreaseFactor, "severityDecreaseFactor", 8.0f);
        Scribe_Values.Look(ref severityIncreaseBias, "severityIncreaseBias", 23.0f);
        Scribe_Values.Look(ref severityFlatBias, "severityFlatBias", 8.0f);
        Scribe_Values.Look(ref severityIncreaseExponent, "severityIncreaseExponent", 1.0f);
        Scribe_Values.Look(ref severityDecreaseExponent, "severityDecreaseExponent", 1.0f);
        Scribe_Values.Look(ref baseMtbDays, "baseMtbDays", 60);


        Scribe_Values.Look(ref factorHealing, "factorHealing", 500f);
        Scribe_Values.Look(ref factorBloodHealing, "factorBloodHealing", 67f);
        Scribe_Values.Look(ref isBloodHealing, "isBloodHealing", false);
        Scribe_Values.Look(ref postFactorHealing, "postFactorHealing", 0.22f);
        Scribe_Values.Look(ref isPostHealing, "isPostHealing", true);


        Scribe_Values.Look(ref mutantFullCircle, "mutantFullCircle", 100.0f);
        Scribe_Values.Look(ref thresholdMinusOne, "thresholdMinusOne", 25);
        Scribe_Values.Look(ref thresholdZero, "thresholdZero", 55);
        Scribe_Values.Look(ref thresholdPlusOne, "thresholdPlusOne", 85);
            

        Scribe_Values.Look(ref inverseTemperature, "inverseTemperature", 5f);
        Scribe_Values.Look(ref allowMutantAssignTab, "allowMutantAssignTab", false);
        Scribe_Values.Look(ref disableTale, "disableTale", false);
        // 临时设置
        base.ExposeData();
    }
}


public class ManosabaMod : Mod
{
    public static ManosabaModSettings Settings;
    public ManosabaMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<ManosabaModSettings>();
    }
    // 特殊静态字符
    private static string YukiNameRaw => "ManosabaSettings_yukiNameDef".Translate();
    public static TaggedString YukiNameDef => YukiNameRaw.Colorize(ColoredText.NameColor);
    private static string EnabledChoiceRaw => "ManosabaSettings_activatedChoice".Translate();
    private static string DisabledChoiceRaw => "ManosabaSettings_deactivatedChoice".Translate();
    public static TaggedString EnabledChoice => EnabledChoiceRaw.Colorize(Color.green);
    public static TaggedString DisabledChoice => DisabledChoiceRaw.Colorize(Color.red);

    // Mod 名称
    public override string SettingsCategory() => "ManosabaSettings_ModName".Translate();

    private readonly Color _lightGray = new Color(0.61f, 0.61f, 0.61f);

    // 多级菜单
    private enum SettingsTab
    {
        GeneralTab,
        ProgressTab,
        HealingTab,
        RitualTab,
        MiscTab
    }
    private SettingsTab _currentTab = SettingsTab.GeneralTab;

    public override void DoSettingsWindowContents(Rect inRect)
    {
        // 顶部留空
        var contentRect = inRect;
        contentRect.yMin += 32f; 
        // 绘制标签栏
        var tabs = new List<TabRecord>
        {
            new TabRecord(
                "ManosabaSettings_GeneralTab".Translate(),
                delegate { _currentTab = SettingsTab.GeneralTab; },
                _currentTab == SettingsTab.GeneralTab
            ),
            new TabRecord(
                "ManosabaSettings_ProgressTab".Translate(),
                delegate { _currentTab = SettingsTab.ProgressTab; },
                _currentTab == SettingsTab.ProgressTab
            ),
            new TabRecord(
                "ManosabaSettings_HealingTab".Translate(),
                delegate { _currentTab = SettingsTab.HealingTab; },
                _currentTab == SettingsTab.HealingTab
            ),
            new TabRecord(
                "ManosabaSettings_RitualTab".Translate(),
                delegate { _currentTab = SettingsTab.RitualTab; },
                _currentTab == SettingsTab.RitualTab
            ),
            new TabRecord(
                "ManosabaSettings_MiscTab".Translate(),
                delegate { _currentTab = SettingsTab.MiscTab; },
                _currentTab == SettingsTab.MiscTab
            ),
        };
        TabDrawer.DrawTabs(contentRect, tabs);
        // 背景框
        Widgets.DrawMenuSection(contentRect);
        // 内容列表
        var innerRect = contentRect.ContractedBy(15f);
        var list = new Listing_Standard();
        list.Begin(innerRect);
        switch (_currentTab)
        {
            case SettingsTab.GeneralTab:
                DrawGeneralTab(list, innerRect);
                break;
            case SettingsTab.ProgressTab:
                DrawProgressTab(list, innerRect);
                break;
            case SettingsTab.HealingTab:
                DrawHealingTab(list, innerRect);
                break;
            case SettingsTab.RitualTab:
                DrawRitualTab(list, innerRect);
                break;
            case SettingsTab.MiscTab:
                DrawMiscTab(list, innerRect);
                break;
        }
        list.End();
    }

    private void DrawGeneralTab(Listing_Standard list, Rect container)
    {
        // 选项: 显示数值
        list.CheckboxLabeled(
            "ManosabaSettings_isShowProgress".Translate(),
            ref Settings.isShowProgress
        );
        list.Indent(20f);
        GUI.color = _lightGray;
        var isShowProgressDesc1 = Settings.isShowProgress ?
            "• " + EnabledChoice + ", " + "ManosabaSettings_isShowProgress_Desc_1".Translate():
            "• " + DisabledChoice + ", " + "ManosabaSettings_isShowProgress_Desc_1_disable".Translate();
        list.Label(isShowProgressDesc1);
        list.Label("ManosabaSettings_isShowProgress_Desc_2".Translate());
        GUI.color = Color.white;
        list.Outdent(20f);
        list.Gap(6f);
        list.GapLine();
        list.Gap(6f);
        //===================================================================
        // 调试面板
        //===================================================================
        string panelTitle = "ManosabaSettings_GeneralPanelTitle".Translate(); 
        var listWidth = list.ColumnWidth;
        var panelWidth = 0.382f * listWidth;
        var titleHeight = Text.CalcHeight(panelTitle, panelWidth);
        var lineHeight = Mathf.Max(30f, titleHeight);
        var rowRectTitle = list.GetRect(3*lineHeight);
        var textRect = new Rect(rowRectTitle.x, rowRectTitle.y, panelWidth, titleHeight);
        Widgets.Label(textRect, panelTitle);
        // 左侧介绍文本
        string leftText = "ManosabaSettings_GeneralPanel_Desc_1".Translate();
        //"ManosabaSettings_GeneralPanel_Desc_2".Translate()
        var leftTextRect = new Rect(rowRectTitle.x + 20f, rowRectTitle.y + titleHeight, panelWidth, 2 * lineHeight);
        GUI.color = _lightGray;
        Widgets.Label(leftTextRect, leftText);
        GUI.color = Color.white;

        //绘制右侧滑条
        var rectRightSlider1 = new Rect(rowRectTitle.xMax - panelWidth, rowRectTitle.y, panelWidth, lineHeight);
        var valRightSlider1 = Settings.previewMood;
        var newValRightSlider1 = Widgets.HorizontalSlider(rectRightSlider1, valRightSlider1, 0f, 100f, false, null, null, null, 1f);
        if (!Mathf.Approximately(newValRightSlider1, valRightSlider1))
        {
            Settings.previewMood = newValRightSlider1;
        }
        var rectRightSlider2 = new Rect(rowRectTitle.xMax - panelWidth, rowRectTitle.y + lineHeight, panelWidth, lineHeight);
        var valRightSlider2 = Settings.previewMinorThres;
        var newValRightSlider2 = Widgets.HorizontalSlider(rectRightSlider2, valRightSlider2, 1f, 49f, false, null, null, null, 1f);
        if (!Mathf.Approximately(newValRightSlider2, valRightSlider2))
        {
            Settings.previewMinorThres = newValRightSlider2;
        }
        var rectRightSlider3 = new Rect(rowRectTitle.xMax - panelWidth, rowRectTitle.y + 2 * lineHeight, panelWidth, lineHeight);
        var valRightSlider3 = Settings.previewSeverity;
        var newValRightSlider3 = Widgets.HorizontalSlider(rectRightSlider3, valRightSlider3, 0f, 100f, false, null, null, null, 1f);
        if (!Mathf.Approximately(newValRightSlider3, valRightSlider3))
        {
            Settings.previewSeverity = newValRightSlider3;
        }
        //绘制右侧文本
        string rightText1 = "ManosabaSettings_GeneralPreviewMood".Translate(Settings.previewMood.ToString("F0"));
        string rightText2 = "ManosabaSettings_GeneralPreviewMinorThres".Translate(Settings.previewMinorThres.ToString("F0"));
        string rightText3 = "ManosabaSettings_GeneralPreviewSeverity".Translate(Settings.previewSeverity.ToString("F0"));
        var rightTextWidth = Mathf.Max(
            Mathf.Max(
                listWidth * 0.236f - 20f,
                Text.CalcSize(rightText1).x
            ),
            Mathf.Max(
                Text.CalcSize(rightText2).x,
                Text.CalcSize(rightText3).x
            )
        );
        var rectRightText1 = new Rect(rowRectTitle.xMax - panelWidth - rightTextWidth + 10f, rowRectTitle.y, panelWidth, lineHeight);
        var rectRightText2 = new Rect(rowRectTitle.xMax - panelWidth - rightTextWidth + 10f, rowRectTitle.y + lineHeight, panelWidth, lineHeight);
        var rectRightText3 = new Rect(rowRectTitle.xMax - panelWidth - rightTextWidth + 10f, rowRectTitle.y + 2 * lineHeight, panelWidth, lineHeight);

        Widgets.Label(rectRightText1, rightText1);
        Widgets.Label(rectRightText2, rightText2);
        Widgets.Label(rectRightText3, rightText3);

        list.Gap(6f);

        // 函数图像
        var graphRect = list.GetRect(200f);
        DrawSeverityCurve(graphRect);
        list.Gap(6f);

        // 数值预览输出
        var rowRectValOutput = list.GetRect(30f);
        var colWidthValOutput = (rowRectValOutput.width - 50f) / 2f;
        var leftRectValOutput = new Rect(rowRectValOutput.x, rowRectValOutput.y, colWidthValOutput, rowRectValOutput.height);
        var rightRectValOutput = new Rect(rowRectValOutput.x + (colWidthValOutput + 50f), rowRectValOutput.y, colWidthValOutput, rowRectValOutput.height);
        // 预览输出左
        var changePerDayPreview = 120*100*Comps.HediffComp_Progress.GetSeverityChange(Settings.previewMood / 100f,  Settings.previewMinorThres / 100f);
        string leftTextValOutput = "ManosabaSettings_changePerDayPreview".Translate(changePerDayPreview.ToString("F4"));
        // 预览输出右
        var mtbDaysPreview = Settings.baseMtbDays 
                             * Mathf.Pow(4f, Mathf.Lerp(1.0f, -1.0f, Settings.previewSeverity / 100f))
                             * Mathf.Pow(2f, Mathf.Lerp(-1.0f, 1.0f, Settings.previewMood / 100f));
        string rightTextValOutput = "ManosabaSettings_mtbDaysPreview".Translate(mtbDaysPreview.ToString("F1"));

        Widgets.Label(leftRectValOutput, leftTextValOutput);
        Widgets.Label(rightRectValOutput, rightTextValOutput);
        list.Gap(6f);
        list.GapLine();
        list.Gap(6f);



        // 开发者模式
        list.CheckboxLabeled(
            "ManosabaSettings_debugMode".Translate(),
            ref Settings.debugMode
        );
        list.Indent(20f);
        GUI.color = _lightGray;
        var buildDate = " (last compiled at " + DateTime.Now.ToString("yyyy-MM-dd") + ")";
        string debugModeDesc = Settings.debugMode ?
            "ManosabaSettings_debugMode_Desc".Translate() + buildDate :
            "ManosabaSettings_debugMode_Desc".Translate();
        list.Label(debugModeDesc);
        GUI.color = Color.white;
        list.Outdent(20f);

    }

    private void DrawProgressTab(Listing_Standard list, Rect container)
    {
        const float listGap = 6f;                        // 列表间距
        const float listGapline = 6f;                      // 列表分割线间距

        // 进度增加系数
        DrawSliderPanels(
            list,
            labelTextTranslateString: "ManosabaSettings_severityIncreaseFactor",
            value: ref Settings.severityIncreaseFactor,
            slideMin: 1f,
            slideMax: 100f,
            roundTo: 1f,
            valueFormat: "F0"
        );
        list.Indent(20f);
        GUI.color = _lightGray;
        list.Label("ManosabaSettings_severityIncreaseFactor_Desc".Translate());
        GUI.color = Color.white;
        list.Outdent(20f);
        list.Gap(listGap);

        // 进度降低系数
        DrawSliderPanels(
            list,
            labelTextTranslateString: "ManosabaSettings_severityDecreaseFactor",
            value: ref Settings.severityDecreaseFactor,
            slideMin: 1f,
            slideMax: 100f,
            roundTo: 1f,
            valueFormat: "F0"
        );
        list.Indent(20f);
        GUI.color = _lightGray;
        list.Label("ManosabaSettings_severityDecreaseFactor_Desc".Translate());
        GUI.color = Color.white;
        list.Outdent(20f);
        list.Gap(listGap);
        list.GapLine();
        list.Gap(listGapline);

        // 上升阈值 相对 Pawn 轻度崩溃 的偏移
        DrawSliderPanels(
            list,
            labelTextTranslateString: "ManosabaSettings_severityIncreaseBias",
            value: ref Settings.severityIncreaseBias,
            slideMin: 1f,
            slideMax: 97f,
            roundTo: 1f,
            valueFormat: "F0"
        );
        list.Indent(20f);
        GUI.color = _lightGray;
        list.Label("ManosabaSettings_severityIncreaseBias_Desc".Translate());
        GUI.color = Color.white;
        list.Outdent(20f);
        list.Gap(listGap);

        // 下降阈值 相对 上升阈值 的偏移
        DrawSliderPanels(
            list,
            labelTextTranslateString: "ManosabaSettings_severityFlatBias",
            value: ref Settings.severityFlatBias,
            slideMin: 0f,
            slideMax: 97f,
            roundTo: 1f,
            valueFormat: "F0"
        );
        list.Indent(20f);
        GUI.color = _lightGray;
        list.Label("ManosabaSettings_severityFlatBias_Desc".Translate());
        GUI.color = Color.white;
        list.Outdent(20f);
        list.Gap(listGap);
        list.GapLine();
        list.Gap(listGapline);

        // 增长因子指数
        DrawSliderPanels(
            list,
            labelTextTranslateString: "ManosabaSettings_severityIncreaseExponent",
            value: ref Settings.severityIncreaseExponent,
            slideMin: 0.10f,
            slideMax: 2.00f,
            roundTo: 0.01f,
            valueFormat: "F2"
        );
        list.Indent(20f);
        GUI.color = _lightGray;
        list.Label("ManosabaSettings_severityIncreaseExponent_Desc".Translate());
        GUI.color = Color.white;
        list.Outdent(20f);
        list.Gap(listGap);

        // 下降因子指数
        DrawSliderPanels(
            list,
            labelTextTranslateString: "ManosabaSettings_severityDecreaseExponent",
            value: ref Settings.severityDecreaseExponent,
            slideMin: 0.10f,
            slideMax: 2.00f,
            roundTo: 0.01f,
            valueFormat: "F2"
        );
        list.Indent(20f);
        GUI.color = _lightGray;
        list.Label("ManosabaSettings_severityDecreaseExponent_Desc".Translate());
        GUI.color = Color.white;
        list.Outdent(20f);
        list.Gap(listGap);
        list.GapLine();
        list.Gap(listGapline);

        // 魔女因子发狂居中期望天数
        DrawSliderPanels(
            list,
            labelTextTranslateString: "ManosabaSettings_baseMtbDays",
            value: ref Settings.baseMtbDays,
            slideMin: 8f,
            slideMax: 120f,
            roundTo: 1f,
            valueFormat: "F0"
        );
        list.Indent(20f);
        GUI.color = _lightGray;
        list.Label("ManosabaSettings_baseMtbDays_Desc_1".Translate());
        list.Label("ManosabaSettings_baseMtbDays_Desc_2".Translate());
        GUI.color = Color.white;
        list.Outdent(20f);
        list.Gap(listGap);
        //list.GapLine();
        //list.Gap(listGapline);

        // 重置本页
        string resetDesc = "ManosabaSettings_ProgressTabDesc".Translate(); 
        string buttonLabel = "ManosabaSettings_ResetProgressTab".Translate();
        // 申请区域
        var labelWidth = Text.CalcSize(buttonLabel).x + 20f; // 按钮标签宽度 + 内边距
        var buttonWidth = Mathf.Max(0.382f * list.ColumnWidth, labelWidth);  // 按钮宽度
        var textWidth = list.ColumnWidth - buttonWidth - 10f;  // 文本宽度
        var textHeight = Text.CalcHeight(resetDesc, textWidth);
        var rowHeight = Mathf.Max(textHeight, 30f);  // 行高度, 至少30f以适应按钮高度
        var bottomMargin = 10f;
        //Rect rowRect = list.GetRect(rowHeight);
        var textRect = new Rect(
            0,
            container.height - rowHeight - bottomMargin,
            textWidth,
            rowHeight
        );
        var buttonRect = new Rect(
            container.width - buttonWidth,
            container.height - rowHeight - bottomMargin,
            buttonWidth,
            rowHeight - 1f
        );
        Widgets.Label(textRect, resetDesc);
        if (Widgets.ButtonText(buttonRect, buttonLabel))
        {
            Settings.severityIncreaseFactor = 16.0f;
            Settings.severityDecreaseFactor = 8.0f;
            Settings.severityIncreaseBias = 23.0f;
            Settings.severityFlatBias = 8.0f;
            Settings.severityIncreaseExponent = 1.0f;
            Settings.severityDecreaseExponent = 1.0f;
            Settings.baseMtbDays = 60;
            SoundDefOf.Click.PlayOneShotOnCamera();
        }

    }

    private void DrawHealingTab(Listing_Standard list, Rect container)
    {
        const float listGap = 6f;
        const float listGapline = 6f;
        // 魔女残骸自愈系数
        DrawSliderPanels(
            list,
            labelTextTranslateString: "ManosabaSettings_factorHealing",
            value: ref Settings.factorHealing,
            slideMin: 100f,
            slideMax: 1000f,
            roundTo: 50f,
            valueFormat: "F0"
        );
        list.Indent(20f);
        GUI.color = _lightGray;
        list.Label("ManosabaSettings_factorHealing_Desc_1".Translate());
        list.Label("ManosabaSettings_factorHealing_Desc_2".Translate(Settings.factorHealing.ToString("F0")));
        GUI.color = Color.white;
        list.Outdent(20f);
        list.Gap(listGap);
        // 魔女残骸失血治疗系数
        DrawSliderPanels(
            list,
            labelTextTranslateString: "ManosabaSettings_factorBloodHealing",
            value: ref Settings.factorBloodHealing,
            slideMin: 0f,
            slideMax: 100f,
            roundTo: 1f,
            valueFormat: "F0"
        );
        Settings.isBloodHealing = Settings.factorBloodHealing != 0f;
        list.Indent(20f);
        GUI.color = _lightGray;
        var bloodHealingDesc1 = Settings.isBloodHealing ?
            "• " + EnabledChoice + ", " + "ManosabaSettings_factorBloodHealing_Desc_1".Translate(Settings.factorBloodHealing.ToString("F0")) :
            "• " + DisabledChoice + ", " + "ManosabaSettings_factorBloodHealing_Desc_1_Disable".Translate();
        list.Label(bloodHealingDesc1);
        list.Label("ManosabaSettings_factorBloodHealing_Desc_2".Translate());
        list.Label("ManosabaSettings_factorBloodHealing_Desc_3".Translate());
        GUI.color = Color.white;
        list.Outdent(20f);
        list.Gap(listGap);
        list.GapLine();
        list.Gap(listGapline);
        // 治愈系数后处理系数
        DrawSliderPanels(
            list,
            labelTextTranslateString: "ManosabaSettings_postFactorHealing",
            value: ref Settings.postFactorHealing,
            slideMin: 0f,
            slideMax: 1f,
            roundTo: 0.01f,
            valueFormat: "F2"
        );
        Settings.isPostHealing = Settings.postFactorHealing != 0f;
        var postHealingEffective = (Settings.postFactorHealing * Settings.factorHealing).ToString("F2");
        var postBloodHealingEffective = (Settings.postFactorHealing * Settings.factorBloodHealing).ToString("F2");
        list.Indent(20f);
        GUI.color = _lightGray;
        var postHealingDesc1 = Settings.isPostHealing ?
            "• " + EnabledChoice + ", " + "ManosabaSettings_postFactorHealing_Desc_1".Translate(Settings.postFactorHealing.ToString("F2"),YukiNameDef.Named("YUKI")) :
            "• " + DisabledChoice + ", " + "ManosabaSettings_postFactorHealing_Desc_1_Disable".Translate(YukiNameDef.Named("YUKI"));
        var postHealingDesc2 = Settings.isPostHealing ?
            "ManosabaSettings_postFactorHealing_Desc_2".Translate(postHealingEffective, postBloodHealingEffective,YukiNameDef.Named("YUKI")) :
            "ManosabaSettings_postFactorHealing_Desc_2_Disable".Translate(YukiNameDef.Named("YUKI"));
        list.Label(postHealingDesc1);
        list.Label(postHealingDesc2);
        GUI.color = Color.white;
        list.Outdent(20f);
        //list.Gap(listGap);
        //list.GapLine();
        //list.Gap(listGapline);

        // 重置本页
        string buttonLabel = "ManosabaSettings_ResetHealingTab".Translate();
        var labelWidth = Text.CalcSize(buttonLabel).x + 20f; // 按钮标签宽度 + 内边距
        var buttonWidth = Mathf.Max(0.382f * list.ColumnWidth, labelWidth);  // 按钮宽度
        var labelHeight = Text.CalcHeight(buttonLabel, labelWidth);
        var rowHeight = Mathf.Max(labelHeight, 30f);  // 行高度, 至少30f以适应按钮高度
        var bottomMargin = 10f;
        //Rect rowRect = list.GetRect(rowHeight);
        var buttonRect = new Rect(
            (container.width - buttonWidth) / 2f, 
            container.height - rowHeight - bottomMargin, 
            buttonWidth, 
            rowHeight-1f            
        );
        if (Widgets.ButtonText(buttonRect, buttonLabel))
        {
            Settings.factorHealing = 500f;
            Settings.factorBloodHealing = 67f;
            Settings.postFactorHealing = 0.22f;
            SoundDefOf.Click.PlayOneShotOnCamera();
        }

    }

    private void DrawRitualTab(Listing_Standard list, Rect container)
    {
        const float listGap = 6f;
        const float listGapline = 6f;
        // 变种人控制时间
        list.Label("ManosabaSettings_NBw_title".Translate());
        list.Indent(20f);
        GUI.color = _lightGray;
        DrawSliderPanels(
            list,
            labelTextTranslateString: "ManosabaSettings_NBw_mutantFullCircle",
            value: ref Settings.mutantFullCircle,
            slideMin: 15f,
            slideMax: 150f,
            roundTo: 1f,
            valueFormat: "F0",
            indent: 20
        );
        list.Indent(20f);
        GUI.color = _lightGray;
        list.Label("ManosabaSettings_NBw_mutantFullCircle_Desc_1".Translate());
        list.Label("ManosabaSettings_NBw_mutantFullCircle_Desc_2".Translate());
        list.Label("ManosabaSettings_NBw_mutantFullCircle_Desc_3".Translate());
        GUI.color = Color.white;
        list.Outdent(40f);
        list.Gap(listGap);
        list.GapLine();
        list.Gap(listGapline);

        // 魔女安息仪式质量偏移阈值
        list.Label("ManosabaSettings_Sabbat_title".Translate());
        list.Indent(20f);
        GUI.color = _lightGray;
        DrawSliderPanels(
            list,
            labelTextTranslateString: "ManosabaSettings_Sabbat_thresholdMinusOne",
            value: ref Settings.thresholdMinusOne,
            slideMin: 0f,
            slideMax: 98f,
            roundTo: 1f,
            valueFormat: "F0",
            indent: 20
        );
        list.Gap(listGap);
        GUI.color = _lightGray;
        DrawSliderPanels(
            list,
            labelTextTranslateString: "ManosabaSettings_Sabbat_thresholdZero",
            value: ref Settings.thresholdZero,
            slideMin: 1f,
            slideMax: 99f,
            roundTo: 1f,
            valueFormat: "F0",
            isConstrained: true,
            minAllowedValue: Settings.thresholdMinusOne + 1f,
            indent: 20
        );
        list.Gap(listGap);
        GUI.color = _lightGray;
        DrawSliderPanels(
            list,
            labelTextTranslateString: "ManosabaSettings_Sabbat_thresholdPlusOne",
            value: ref Settings.thresholdPlusOne,
            slideMin: 2f,
            slideMax: 100f,
            roundTo: 1f,
            valueFormat: "F0",
            isConstrained: true,
            minAllowedValue: Settings.thresholdZero + 1f,
            indent: 20
        );
        list.Indent(20f);
        GUI.color = _lightGray;
        list.Label("ManosabaSettings_Sabbat_thresholds_Desc_1".Translate(YukiNameDef.Named("YUKI")));
        list.Label("ManosabaSettings_Sabbat_thresholds_Desc_2".Translate());
        list.Label("ManosabaSettings_Sabbat_thresholds_Desc_3".Translate(YukiNameDef.Named("YUKI")));
        GUI.color = Color.white;
        list.Outdent(40f);
        //list.Gap(listGap);


        // 重置本页
        string buttonLabel = "ManosabaSettings_ResetRitualTab".Translate();
        var labelWidth = Text.CalcSize(buttonLabel).x + 20f; // 按钮标签宽度 + 内边距
        var buttonWidth = Mathf.Max(0.382f * list.ColumnWidth, labelWidth);  // 按钮宽度
        var labelHeight = Text.CalcHeight(buttonLabel, labelWidth);
        var rowHeight = Mathf.Max(labelHeight, 30f);  // 行高度, 至少30f以适应按钮高度
        var bottomMargin = 10f;
        var buttonRect = new Rect(
            (container.width - buttonWidth) / 2f, 
            container.height - rowHeight - bottomMargin, 
            buttonWidth, 
            rowHeight-1f            
        );
        if (Widgets.ButtonText(buttonRect, buttonLabel))
        {
            Settings.mutantFullCircle = 100f;
            Settings.thresholdMinusOne = 25f;
            Settings.thresholdZero = 55f;
            Settings.thresholdPlusOne = 85f;
            SoundDefOf.Click.PlayOneShotOnCamera();
        }

    }

    private void DrawMiscTab(Listing_Standard list, Rect container)
    {
        const float listGap = 6f;
        const float listGapline = 6f;

        // 随机分布 - 逆温度
        DrawSliderPanels(
            list,
            labelTextTranslateString: "ManosabaSettings_inverseTemperature",
            value: ref Settings.inverseTemperature,
            slideMin: -10f,
            slideMax: 10f,
            roundTo: 0.1f,
            valueFormat: "F1"
        );
        list.Indent(20f);
        GUI.color = _lightGray;
        list.Label("ManosabaSettings_inverseTemperature_Desc_1".Translate());
        list.Label("ManosabaSettings_inverseTemperature_Desc_2".Translate());
        if (Settings.debugMode)
        {
            list.Label("ManosabaSettings_inverseTemperature_Desc_3".Translate());
            list.Label("ManosabaSettings_inverseTemperature_Desc_4".Translate());
        }
        GUI.color = Color.white;
        list.Outdent(20f);
        list.Gap(listGap);
        list.GapLine();
        list.Gap(listGapline);
        
        // 启用魔女残骸的方案管理
        list.CheckboxLabeled(
            "ManosabaSettings_allowMutantAssignTab".Translate(),
            ref Settings.allowMutantAssignTab
        );
        list.Indent(20f);
        GUI.color = _lightGray;
        list.Label("ManosabaSettings_allowMutantAssignTab_Desc_1".Translate());
        list.Label("ManosabaSettings_allowMutantAssignTab_Desc_2".Translate());
        GUI.color = Color.white;
        list.Outdent(20f);
        list.Gap(listGap);
        list.GapLine();
        list.Gap(listGapline);
        
        // 禁用调用故事生成
        list.CheckboxLabeled(
            "ManosabaSettings_disableTale".Translate(),
            ref Settings.disableTale
        );
        list.Indent(20f);
        GUI.color = _lightGray;
        list.Label("ManosabaSettings_disableTale_Desc_1".Translate());
        GUI.color = Color.white;
        list.Outdent(20f);
        list.Gap(listGap);


        // 重置本页
        string buttonLabel = "ManosabaSettings_ResetMiscTab".Translate();
        var labelWidth = Text.CalcSize(buttonLabel).x + 20f; // 按钮标签宽度 + 内边距
        var buttonWidth = Mathf.Max(0.382f * list.ColumnWidth, labelWidth);  // 按钮宽度
        var labelHeight = Text.CalcHeight(buttonLabel, labelWidth);
        var rowHeight = Mathf.Max(labelHeight, 30f);  // 行高度, 至少30f以适应按钮高度
        var bottomMargin = 10f;
        var buttonRect = new Rect(
            (container.width - buttonWidth) / 2f, 
            container.height - rowHeight - bottomMargin, 
            buttonWidth, 
            rowHeight-1f            
        );
        if (Widgets.ButtonText(buttonRect, buttonLabel))
        {
            Settings.inverseTemperature = 5f;
            SoundDefOf.Click.PlayOneShotOnCamera();
        }
    }
        







    private void DrawSliderPanels(
        Listing_Standard list,
        string labelTextTranslateString,
        ref float value,
        float slideMin, float slideMax,
        float roundTo = 1f, string valueFormat = "F0",
        float minSliderWidth = 200f,
        float percentSliderWidth = 0.382f,
        float gapTextSlider = 6f,
        bool isConstrained = false,
        float minAllowedValue = float.NegativeInfinity,
        float maxAllowedValue = float.PositiveInfinity,
        int indent = 0
    )
    {
        string labelText = labelTextTranslateString.Translate(value.ToString(valueFormat));
        var currentVal = value;
        // 长宽计算
        var totalWidth = list.ColumnWidth;
        var sliderWidth = Mathf.Max( percentSliderWidth * totalWidth, minSliderWidth);  // 滑条宽度
        var textWidth = totalWidth - sliderWidth - gapTextSlider - indent;  // 文本宽度
        var textHeight = Text.CalcHeight(labelText, textWidth);  // 文本高度
        var rowHeight = Mathf.Max(textHeight, 24f); // 最小行高度
        // 绘制
        var rowRect = list.GetRect(rowHeight);
        var textRect = new Rect(rowRect.x, rowRect.y, textWidth, rowHeight);
        var sliderRect = new Rect(rowRect.xMax - sliderWidth - indent, rowRect.y, sliderWidth, 24f);
        Widgets.Label(textRect, labelText);
        var newVal = Widgets.HorizontalSlider(sliderRect, currentVal, slideMin, slideMax, false, null, null, null, roundTo);
        if (isConstrained)
        {
            newVal = Mathf.Min(Mathf.Max(newVal, minAllowedValue), maxAllowedValue);
        }
        if (!Mathf.Approximately(newVal, currentVal))
        {
            value = newVal;
        }
    }
    private void DrawSeverityCurve(Rect rect, float yEdgePercent = 0.1f, int segments = 101)
    {
        // 绘制区域
        Widgets.DrawBoxSolid(rect, new Color(0.1f, 0.1f, 0.1f));
        Widgets.DrawBox(rect);
        Vector2? lastPoint = null;
        // 采样函数
        const float xMax = 100f;
        const float xMin = 0f;
        var xThresIncrease = Mathf.Max(1, Mathf.Min(Settings.previewMinorThres + Settings.severityIncreaseBias, 98));
        var xThresDecrease = Mathf.Max(2, Mathf.Min(xThresIncrease + Settings.severityFlatBias, 99));
        var yValRange = Settings.severityIncreaseFactor + Settings.severityDecreaseFactor;
        var yDrawLength = rect.height / (1 + 2* yEdgePercent);
        // 颜色设置
        var originalColor = GUI.color;
        GUI.color = _lightGray;
        Color LineColor(float x) => 
            ( x <= xThresIncrease+1f )? Color.red:
            ( x >= xThresDecrease )? Color.green:
            Color.yellow;
        // --- 完整曲线 ---
        for (var i = 0; i <= segments; i++)
        {
            var t = (float)i / segments;
            var mathX = Mathf.Lerp(xMin, xMax, t); // X 轴取值范围 [xMin, xMax]
            var mathY = 120*100*Comps.HediffComp_Progress.GetSeverityChange( mathX/100f,  Settings.previewMinorThres / 100f);
            var guiX = rect.x + t * rect.width;
            var normalizedY = (Settings.severityIncreaseFactor - mathY) / yValRange; // 函数值 -[-sDF,sIF] 映射为 [0,1]
            var guiY = rect.y + yDrawLength * (yEdgePercent + normalizedY);
            var currentPoint = new Vector2(guiX, guiY);
            if (lastPoint.HasValue)
            {
                Widgets.DrawLine(lastPoint.Value, currentPoint, LineColor(mathX), 2f);
            }
            lastPoint = currentPoint;
        }
        // --- 当前点 ---
        if (Settings.previewMood is >= xMin and <= xMax)
        {
            var mathX = Settings.previewMood;
            var mathY = 120*100*Comps.HediffComp_Progress.GetSeverityChange( mathX/100f,  Settings.previewMinorThres / 100f);
            var t = (mathX - xMin) / (xMax - xMin);
            var guiX = rect.x + t * rect.width;
            var normalizedY = (Settings.severityIncreaseFactor - mathY) / yValRange;
            var guiY = rect.y + yDrawLength * (yEdgePercent + normalizedY);
            var circSize = 8f;
            var circRect = new Rect(
                guiX - circSize / 2f, 
                guiY - circSize / 2f, 
                circSize, 
                circSize
            );
            var oldColor = GUI.color;
            GUI.color = Color.cyan;
            GUI.DrawTexture(circRect, BaseContent.WhiteTex); 
            GUI.color = oldColor;
        }
        // --- 绘制y轴 ---
        GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        var guiYZero = rect.y + yDrawLength * (yEdgePercent + Settings.severityIncreaseFactor / yValRange);
        Widgets.DrawLine(new Vector2(rect.x, guiYZero), new Vector2(rect.xMax, guiYZero), GUI.color, 1f);
        // --- 文字标签 ---
        GUI.color = originalColor;
        Text.Font = GameFont.Tiny;
        GUI.color = _lightGray;
        // y轴最大值
        Widgets.Label(new Rect(rect.x + 4, rect.y, 100, 20), $"Max: {Settings.severityIncreaseFactor}");
        // 原点
        Widgets.Label(new Rect(rect.x + 4, guiYZero - 20, 100, 20), "0");
        // y轴最小值
        Widgets.Label(new Rect(rect.x + 4, rect.yMax -20, 100, 20), $"Min: {-Settings.severityDecreaseFactor}");
        // x轴最大值
        Widgets.Label(new Rect(rect.xMax - 30, rect.yMax - 20, 30, 20), $"{xMax}%");
        // --- 数据标记 ---
        var oldAnchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleCenter;
        // x轴增加阈值
        string thresIncLabel = "ManosabaSettings_drawIncreaseThreshold".Translate(xThresIncrease.ToString("F0"));
        Widgets.Label(new Rect(
                rect.x -50 + xThresIncrease/100f * rect.width,
                rect.y + 4,
                100, 40), thresIncLabel
        );
        // x轴减小阈值
        string thresDecLabel = "ManosabaSettings_drawDecreaseThreshold".Translate(xThresDecrease.ToString("F0"));
        Widgets.Label(new Rect(
                rect.x + -50 + xThresDecrease/100f * rect.width,
                rect.y + 4,
                100, 40), thresDecLabel
        );
        // 恢复设置
        Text.Anchor = oldAnchor;
        GUI.color = Color.white;
        Text.Font = GameFont.Small;
    }
}