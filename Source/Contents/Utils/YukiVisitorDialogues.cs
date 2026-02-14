using Verse;

namespace UranvManosaba.Contents.Utils;

public class YukiVisitorDialogues
{
    public YukiVisitorDialogues(Pawn visitor)
    {
        // 通用退出选项
        LevelChoiceQuit = "Manosaba_CompYukiVisitor_level_choice_quit".Translate(); // 通用 - 选项 3【Colonist>>Yuki】:退出对话
        // 末态分支
        LevelInf1 = "Manosaba_CompYukiVisitor_level_inf_1".Translate(visitor); // 会话 无限循环 1【Yuki>>Colonist】:
        LevelInf2 = "Manosaba_CompYukiVisitor_level_inf_2".Translate(visitor); // 会话 无限循环 2【Yuki>>Colonist】:
        LevelInf3 = "Manosaba_CompYukiVisitor_level_inf_3".Translate(visitor); // 会话 无限循环 3【Yuki>>Colonist】:
        LevelInfChoice1 = "Manosaba_CompYukiVisitor_level_inf_choice_1".Translate(); // 会话 无限循环 - 选项 1【Colonist>>Yuki】:继续
        LevelInfChoiceTale = "Manosaba_CompYukiVisitor_level_inf_choice_tale".Translate(visitor); // 会话 无限循环 - 选项 故事【Colonist>>Yuki】:讲故事
        // 层级 1
        Level1 = "Manosaba_CompYukiVisitor_level_1".Translate(visitor); // 会话 1【Yuki>>Colonist】:
        Level1Choice1 = "Manosaba_CompYukiVisitor_level_1_choice_1".Translate(); // 会话 1 - 选项 1【Colonist>>Yuki】:确认
        Level1Choice2 = "Manosaba_CompYukiVisitor_level_1_choice_2".Translate(); // 会话 1 - 选项 2【Colonist>>Yuki】:否定
        // 层级 2
        Level2 = "Manosaba_CompYukiVisitor_level_2".Translate(visitor); // 会话 1【Yuki>>Colonist】:
        Level2Choice1 = "Manosaba_CompYukiVisitor_level_2_choice_1".Translate(); // 会话 2 - 选项 1【Colonist>>Yuki】
        Level2Choice2 = "Manosaba_CompYukiVisitor_level_2_choice_2".Translate(); // 会话 2 - 选项 2【Colonist>>Yuki】
        // 层级 3
        Level3 = "Manosaba_CompYukiVisitor_level_3".Translate(visitor); // 会话 3【Yuki>>Colonist】:
        Level3Choice1 = "Manosaba_CompYukiVisitor_level_3_choice_1".Translate(); // 会话 3 - 选项 1【Colonist>>Yuki】
        Level3Choice2 = "Manosaba_CompYukiVisitor_level_3_choice_2".Translate(); // 会话 3 - 选项 2【Colonist>>Yuki】
        Level3Choice3 = "Manosaba_CompYukiVisitor_level_3_choice_3".Translate(); // 会话 3 - 选项 3【Colonist>>Yuki】
        Level3Choice4 = "Manosaba_CompYukiVisitor_level_3_choice_4".Translate(); // 会话 3 - 选项 4【Colonist>>Yuki】
        // 层级 4
        Level4 = "Manosaba_CompYukiVisitor_level_4".Translate(visitor); // 会话 4【Yuki>>Colonist】:
        Level4Choice1 = "Manosaba_CompYukiVisitor_level_4_choice_1".Translate(); // 会话 4 - 选项 1【Colonist>>Yuki】
        // 层级 5
        Level5 = "Manosaba_CompYukiVisitor_level_5".Translate(visitor); // 会话 5【Yuki>>Colonist】:
        Level5Choice1 = "Manosaba_CompYukiVisitor_level_5_choice_1".Translate(); // 会话 5 - 选项 1【Colonist>>Yuki】
        Level5Choice2 = "Manosaba_CompYukiVisitor_level_5_choice_2".Translate(); // 会话 5 - 选项 2【Colonist>>Yuki】
        // 层级 6
        Level6 = "Manosaba_CompYukiVisitor_level_6".Translate(visitor); // 会话 6【Yuki>>Colonist】:
        Level6Choice1 = "Manosaba_CompYukiVisitor_level_6_choice_1".Translate(); // 会话 6 - 选项 1【Colonist>>Yuki】
        // 层级 10
        Level10 = "Manosaba_CompYukiVisitor_level_10".Translate(visitor); // 会话 10【Yuki>>Colonist】:
        Level10Choice1 = "Manosaba_CompYukiVisitor_level_10_choice_1".Translate(); // 会话 10 - 选项 1【Colonist>>Yuki】
        Level10Choice2 = "Manosaba_CompYukiVisitor_level_10_choice_2".Translate(); // 会话 10 - 选项 2【Colonist>>Yuki】
        Level10Choice3 = "Manosaba_CompYukiVisitor_level_10_choice_3".Translate(); // 会话 10 - 选项 3【Colonist>>Yuki】
        // 层级 11
        Level11 = "Manosaba_CompYukiVisitor_level_11".Translate(visitor); // 会话 11【Yuki>>Colonist】:
        Level11Choice1 = "Manosaba_CompYukiVisitor_level_11_choice_1".Translate(); // 会话 11 - 选项 1【Colonist>>Yuki】
    }
    
    // 通用退出选项
    public TaggedString LevelChoiceQuit { get; private set; }
    // 最终循环选项
    public TaggedString LevelInf1 { get; private set; }
    public TaggedString LevelInf2 { get; private set; }
    public TaggedString LevelInf3 { get; private set; }
    public TaggedString LevelInfChoice1 { get; private set; }
    public TaggedString LevelInfChoiceTale { get; private set; }
    // 层级 1
    public TaggedString Level1 { get; private set; }
    public TaggedString Level1Choice1 { get; private set; }
    public TaggedString Level1Choice2 { get; private set; }
    // 层级 2
    public TaggedString Level2 { get; private set; }
    public TaggedString Level2Choice1 { get; private set; }
    public TaggedString Level2Choice2 { get; private set; }
    // 层级 2
    public TaggedString Level3 { get; private set; }
    public TaggedString Level3Choice1 { get; private set; }
    public TaggedString Level3Choice2 { get; private set; }
    public TaggedString Level3Choice3 { get; private set; }
    public TaggedString Level3Choice4 { get; private set; }
    // 层级 4
    public TaggedString Level4 { get; private set; }
    public TaggedString Level4Choice1 { get; private set; }
    // 层级 5
    public TaggedString Level5 { get; private set; }
    public TaggedString Level5Choice1 { get; private set; }
    public TaggedString Level5Choice2 { get; private set; }
    // 层级 6
    public TaggedString Level6 { get; private set; }
    public TaggedString Level6Choice1 { get; private set; }
    // 层级 10
    public TaggedString Level10 { get; private set; }
    public TaggedString Level10Choice1 { get; private set; }
    public TaggedString Level10Choice2 { get; private set; }
    public TaggedString Level10Choice3 { get; private set; }
    // 层级 11
    public TaggedString Level11 { get; private set; }
    public TaggedString Level11Choice1 { get; private set; }
}