namespace project1.addons.sbgoap.ai.schedule;

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