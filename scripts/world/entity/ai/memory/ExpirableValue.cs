namespace project1.scripts.world.entity.ai.memory;

public struct ExpirableValue<TYPE>
{
    public readonly TYPE Value;
    public long TimeToLive { get; set; }
    public bool IsExpired => TimeToLive <= 0;
    public bool IsExpirable => TimeToLive != long.MaxValue;

    public ExpirableValue(TYPE value, long timeToLive = long.MaxValue)
    {
        Value = value;
        TimeToLive = timeToLive;
    }

    public void Update()
    {
        if (IsExpirable) TimeToLive--;
    }
}