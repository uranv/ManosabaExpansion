using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace UranvManosaba.Contents.Comps;

public class Command_SwitchableAbility : Command_Ability
{
    public Command_SwitchableAbility(Ability ability, Pawn pawn) : base(ability, pawn)
    {
    }

    public override void ProcessInput(Event ev)
    {
        if (ev is not { button: 1 })
        {
            base.ProcessInput(ev);
        }
    }
        
    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        var rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
        var ev = Event.current;
        if (ev.type != EventType.MouseDown || ev.button != 1 || !Mouse.IsOver(rect))
        {
            return base.GizmoOnGUI(topLeft, maxWidth, parms);
        }
        SoundDefOf.FloatMenu_Open.PlayOneShotOnCamera();
        SwitchToNextAbility();
        ev.Use();
        base.GizmoOnGUI(topLeft, maxWidth, parms);
        return new GizmoResult(GizmoState.Clear);
    }
    
    private void SwitchToNextAbility()
    {
        var currentAbility = this.Ability;
        var currentComp = currentAbility.CompOfType<AbilityComp_AbilityGroup>();
        if (currentComp == null) return;

        string myGroupId = currentComp.Props.groupId;
        var pawn = currentAbility.pawn;
            
        var allAbilities = pawn.abilities.abilities; 
        var groupAbilities = new List<Ability>();

        foreach (var ab in allAbilities)
        {
            var comp = ab.CompOfType<AbilityComp_AbilityGroup>();
            if (comp != null && comp.Props.groupId == myGroupId)
            {
                groupAbilities.Add(ab);
            }
        }
            
        groupAbilities.SortBy(a => a.def.defName);

        if (groupAbilities.Count <= 1) return;
            
        var index = groupAbilities.IndexOf(currentAbility);
        var nextIndex = (index + 1) % groupAbilities.Count;
        var nextAbility = groupAbilities[nextIndex];
        foreach (var ab in groupAbilities)
        {
            var comp = ab.CompOfType<AbilityComp_AbilityGroup>();
            if (comp == null) continue;
            comp.isSelected = ab == nextAbility;
        }

    }
}