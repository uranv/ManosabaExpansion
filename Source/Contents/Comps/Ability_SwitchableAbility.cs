using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Comps;

public class Ability_SwitchableAbility : Ability
{
    public Ability_SwitchableAbility() : base() { }
    public Ability_SwitchableAbility(Pawn pawn) : base(pawn) { }
    public Ability_SwitchableAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        
    public override IEnumerable<Command> GetGizmos()
    {
        var groupComp = CompOfType<AbilityComp_AbilityGroup>();
        if (groupComp is { isSelected: false }) 
            yield break;
        foreach (var cmd in base.GetGizmos())
        {
            yield return cmd;
        }
    }
}