using RimWorld;
using Verse;

namespace UranvManosaba.Contents;

[DefOf]
public static class ModDefOf
{
    // HediffDefs
    public static HediffDef UmHediffHumanDummy;
    public static HediffDef UmHediffHumanVisible;
    public static HediffDef UmHediffNarehate;
    public static HediffDef UmHediffNarehateHidden;
    public static HediffDef UmHediffNarehateHealed;
    public static HediffDef UmHediffMutantDummy;
    public static HediffDef UmHediffMutantCountdown;
    public static HediffDef UmHediffMutantDraft;
    public static HediffDef UmHediffYukiDummy;
    public static HediffDef UmHediffYukiVisitor;
    public static HediffDef UmHediffYukiVisitorHidden;
    public static HediffDef UmHediffYukiVisitorInvisiblity;
    public static HediffDef UmHediffYukiColonist;
    public static HediffDef UmHediffYukiColonistHidden;
    public static HediffDef UmHediffYukiSkipDebuff;
    public static HediffDef UmHediffTredecim;
    public static HediffDef UmHediffCrystallized;

    // ResearchProjectDefs
    public static ResearchProjectDef UmResearchProjectPrereqs;
    public static ResearchProjectDef UmResearchProjectDetect;
    public static ResearchProjectDef UmResearchProjectKill;
    public static ResearchProjectDef UmResearchProjectControl;
    public static ResearchProjectDef UmResearchProjectGene;
    public static ResearchProjectDef UmResearchProjectSabbat;
    
    // ThingDefs
    public static ThingDef UmThingAburibin;
    public static ThingDef UmThingRaceYuki;
    public static ThingDef UmThingApparelYukiDress;
    public static ThingDef UmThingWeaponSimpleSpear;
    public static ThingDef UmThingWeaponRitualSword;
    public static ThingDef UmThingWeaponRitualSwordReplica;
    public static ThingDef UmThingMoteSabbat;
    public static FleckDef UmFleckSabbatBlood;
    public static SoundDef UmSound_gDie_Divil_JIO;
    
    // MutantDefs
    public static MutantDef UmMutantNarehate;

    // HeadTypeDefs
    public static HeadTypeDef UmHeadTypeYuki;
    
    // BackStoryDefs
    public static BackstoryDef UmBackstoryYukiChild;
    public static BackstoryDef UmBackstoryYukiAdult;
    
    // PawnKindDefs
    public static PawnKindDef UmPawnKindYukiVisitor;
    public static PawnKindDef UmPawnKindYukiColonist;
    
    // ThoughtDefs
    public static ThoughtDef UmThoughtYukiLeaveInstantly;
    
    // RulePackDefs
    public static RulePackDef UmRulePackStory;
    public static RulePackDef UmRulePackTale;
    public static RulePackDef UmRulePackUtils;
    
    // JobDefs
    public static JobDef UmJobYukiVisitorCast;
    public static JobDef UmJobYukiVisitorCastHidden;
    
    // MentalStateDefs
    public static MentalStateDef UmMentalBreakFactor;
    public static MentalStateDef UmMentalBreakNarehate;

    // GeneDefs
    [MayRequireBiotech]
    public static GeneDef UmGeneFactor;
    
    static ModDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(ModDefOf));
    }
}