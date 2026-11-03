using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class CompProperties_AbilityGroup : AbilityCompProperties
{
    public string GroupId = "DefaultGroup";
    public bool IsDefault = false;

    public CompProperties_AbilityGroup()
    {
        compClass = typeof(AbilityComp_AbilityGroup);
    }
}

public class AbilityComp_AbilityGroup : AbilityComp
{
    public CompProperties_AbilityGroup Props => (CompProperties_AbilityGroup)props;
    public bool IsSelected;

    public override void Initialize(AbilityCompProperties p)
    {
        base.Initialize(p);
        if (Scribe.mode == LoadSaveMode.Inactive && Props.IsDefault)
        {
            IsSelected = true;
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref IsSelected, "IsSelected", Props.IsDefault);
    }
}