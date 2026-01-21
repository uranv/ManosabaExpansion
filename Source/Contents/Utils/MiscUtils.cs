using RimWorld;
using Verse;

namespace UranvManosaba.Contents.Utils;

public static class MiscUtils
{
    /// <summary>
    /// 将 HediffDef 添加至部位
    /// </summary>
    /// <param name="pawn"></param>
    /// <param name="hediffDef"></param>
    /// <param name="partDef"></param>
    /// <param name="severity"></param>
    public static void AddHediffTo(Pawn pawn, HediffDef hediffDef, BodyPartDef partDef, float severity = -1.0f)
    {
        if (pawn == null || hediffDef == null) return;
        if (partDef == null)
        {
            var hediff = pawn.health.AddHediff(hediffDef, null);
            if (severity is > 0f and < 1f)
            {
                hediff.Severity = severity;
            }
            return;
        }
        var part = pawn.health.hediffSet.GetNotMissingParts()
            .FirstOrDefault(x => x.def == partDef);
        if (part != null)
        {
            var hediff = pawn.health.AddHediff(hediffDef, part);
            if (severity is > 0f and < 1f)
            {
                hediff.Severity = severity;
            }
        }
    }
        
    /// <summary>
    /// 将 HediffDef 添加到所有原生的眼睛部位
    /// </summary>
    public static void AddHediffToNaturalEyes(Pawn pawn, HediffDef hediffDef)
    {
        if (pawn == null || hediffDef == null) return;
        var currentParts = pawn.health.hediffSet.GetNotMissingParts();
        var eyes = currentParts.Where(p => p.def == BodyPartDefOf.Eye);
        foreach (var eye in eyes)
        {
            var isBionic = pawn.health.hediffSet.hediffs.Any(h => 
                h.Part == eye && 
                h is Hediff_AddedPart
            );
            if (!isBionic)
            {
                pawn.health.AddHediff(hediffDef, eye);
            }
        }
    }

    /// <summary>
    /// Pawn 脏数据基本检查
    /// </summary>
    public static void SanityCheckPawnData(Pawn p, string debugRole = "Pawn")
    {
        if (p == null) 
        {
            Log.Error($"[Manosaba] Error: {debugRole} is Null (Utils.MiscUtils.SanityCheckPawnData)"); 
            return; 
        }
            
        if (p.Name == null)
        {
            Log.Warning($"[Manosaba] FIXING CRASH: {debugRole} has Null NAME (Utils.MiscUtils.SanityCheckPawnData)");
            if (p.RaceProps.Humanlike)
                p.Name = new NameTriple("Null", p.LabelShort ?? "ERR", "Null");
            else
                p.Name = new NameSingle(p.LabelShort ?? "ERR");
        }
            
        if (p.kindDef == null)
        {
            Log.Warning($"[Manosaba] FIXING CRASH: {debugRole} (ID:{p.ThingID}) has Null KindDef (Utils.MiscUtils.SanityCheckPawnData)");
            p.kindDef = PawnKindDefOf.Colonist;
        }

        if (p.ageTracker == null)
        {
            Log.Warning($"[Manosaba] FIXING CRASH: {debugRole} (ID:{p.ThingID}) has Null AgeTracker (Utils.MiscUtils.SanityCheckPawnData)");
            p.ageTracker = new Pawn_AgeTracker(p)
            {
                AgeBiologicalTicks = 3600000L * 18,
                AgeChronologicalTicks = 3600000L * 18
            };
        }
            
        if (p.relations == null)
        {
            Log.Warning($"[Manosaba] FIXING CRASH: {debugRole} (ID:{p.ThingID}) has Null RelationsTracker (Utils.MiscUtils.SanityCheckPawnData)");
            p.relations = new Pawn_RelationsTracker(p);
        }
            
        if (p.RaceProps.Humanlike && p.story == null)
        {
            Log.Warning($"[Manosaba] FIXING CRASH: {debugRole} (ID:{p.ThingID}) has Null StoryTracker (Utils.MiscUtils.SanityCheckPawnData)");
            p.story = new Pawn_StoryTracker(p)
            {
                Childhood = DefDatabase<BackstoryDef>.AllDefsListForReading.FirstOrDefault()
            };
        }
    }
}