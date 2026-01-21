using Verse;

namespace UranvManosaba.Contents.Patches;

[StaticConstructorOnStartup]
public static class UranvManosaba_Main
{
    static UranvManosaba_Main()
    {
        var harmony = new HarmonyLib.Harmony("com.uranv.manosaba");
        harmony.PatchAll();
    }
}