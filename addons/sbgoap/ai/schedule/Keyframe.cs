namespace project1.addons.sbgoap.ai.schedule;

public struct Keyframe
{
    public readonly ulong TimeStamp;
    public readonly float Value;

    public Keyframe(ulong timeStamp, float value)
    {
        TimeStamp = timeStamp;
        Value = value;
    }
}