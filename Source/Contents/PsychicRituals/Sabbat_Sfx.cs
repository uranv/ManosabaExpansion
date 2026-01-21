using Verse;
using Verse.Sound;

namespace UranvManosaba.Contents.PsychicRituals;

public static class Sabbat_Sfx
{
    public static Sustainer CreateSustainer(Verse.AI.Group.PsychicRitual ritual)
    {
        if (ritual == null)  return null;
        var musicDef = ModDefOf.UmSound_gDie_Divil_JIO;
        var musicSustainer = musicDef.TrySpawnSustainer(ritual.assignments.Target);
        
        if (musicSustainer != null)
        {
            musicSustainer.externalParams["MusicVolume"] = Prefs.VolumeMusic;
        }
        return musicSustainer;
        
    }
    public static void StopSustainer(Sustainer musicSustainer)
    {
        if (musicSustainer == null || musicSustainer.Ended) return;
        musicSustainer.End();
    }
}