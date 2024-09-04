namespace project1.scripts.world.entity.ai.schedule;

public struct Keyframe
{
    public readonly int TimeStamp;
    public readonly float Value;
    
    public Keyframe(int timeStamp, float value)
    {
        TimeStamp = timeStamp;
        Value = value;
    }
}