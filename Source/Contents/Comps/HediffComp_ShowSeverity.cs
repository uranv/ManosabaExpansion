using Verse;

namespace UranvManosaba.Contents.Comps;

public class HediffCompProperties_ShowSeverity : HediffCompProperties
{
    public HediffCompProperties_ShowSeverity()
    {
        compClass = typeof(HediffComp_ShowSeverity);
    }
}
public class HediffComp_ShowSeverity : HediffComp
{
    public override string CompLabelInBracketsExtra => ManosabaMod.Settings.isShowProgress ? parent.Severity.ToString("P1") : base.CompLabelInBracketsExtra;
}