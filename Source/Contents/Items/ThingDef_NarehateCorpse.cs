using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Items;

public class ThingDef_NarehateCorpse : ThingDef
{
    public override void ResolveReferences()
    {
        var human = ThingDefOf.Human;
        race = human.race;
        this.SetStatBaseValue(StatDefOf.MaxHitPoints, human.BaseMaxHitPoints);

        var tabs = human.race.corpseDef.inspectorTabs?.ToList() ?? new List<Type>();
        if (inspectorTabs != null)
        {
            foreach (var tab in inspectorTabs)
            {
                if (!tabs.Contains(tab)) tabs.Add(tab);
            }
        }
        inspectorTabs = tabs;

        base.ResolveReferences();
    }
}
