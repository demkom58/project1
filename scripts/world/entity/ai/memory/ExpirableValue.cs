using System;

namespace project1.scripts.world.entity.ai.memory;

public struct ExpirableValue<TYPE>
{
    public readonly TYPE Value;
    public long TimeToLive { get; set; }
    public bool IsExpired => TimeToLive <= 0;
    public bool IsExpirable => TimeToLive != Int64.MaxValue;
    
    public ExpirableValue(TYPE value, long timeToLive = Int64.MaxValue)
    {
        Value = value;
        TimeToLive = timeToLive;
    }

    public void Update()
    {
        if (IsExpirable)
        {
            TimeToLive--;
        }
    }
}