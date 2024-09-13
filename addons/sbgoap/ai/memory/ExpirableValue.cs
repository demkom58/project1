namespace project1.addons.sbgoap.ai.memory;

public class ExpirableValue<TYPE>
{
    public readonly TYPE? Value;

    public ExpirableValue(TYPE? value = default, long timeToLive = long.MaxValue)
    {
        Value = value;
        TimeToLive = timeToLive;
    }

    public long TimeToLive { get; set; }
    public bool IsExpired => TimeToLive <= 0;
    public bool IsExpirable => TimeToLive != long.MaxValue;

    public void Update()
    {
        if (IsExpirable) TimeToLive--;
    }
}