using UnityEngine;
using Verse;

namespace UranvManosaba.Contents.Visuals;

public class ModExtension_HairExtra : DefModExtension
{
    [NoTranslate]
    public string texPathBack;

    public Vector2 drawSize = Vector2.one;
    public float layerFront = 62f;
    public float layerBack = -0.1f;
}
