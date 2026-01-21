using Verse;

namespace UranvManosaba.Contents.PsychicRituals;

public class MoteThrown_Sabbat : MoteThrown
{
    private const float AccelerationX = 0.0001f; 
        
    protected override void TimeInterval(float deltaTime)
    {
        base.TimeInterval(deltaTime);
        if (Destroyed) return;
            
        if (airTimeLeft > def.mote.fadeOutTime)
        {
            velocity.x += 20 * AccelerationX * deltaTime; 
        }
        else
        {
            velocity.x -= AccelerationX * deltaTime;
        }
            
    }
}