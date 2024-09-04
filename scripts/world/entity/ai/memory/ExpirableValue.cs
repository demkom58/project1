using System;

namespace project1.scripts.world.entity.ai.memory;

public struct ExpirableValue<TYPE>
{
    public readonly TYPE Value;
    public long TimeToLive { get; set; }
    
    public ExpirableValue(TYPE value, long timeToLive = Int64.MaxValue)
    {
        Value = value;
        TimeToLive = timeToLive;
    }

    public void Update()
    {
        if (CanExpire())
        {
            TimeToLive--;
        }
    }
    
    public bool IsExpired(long currentTime)
    {
        return currentTime >= TimeToLive;
    }
    
    public bool CanExpire()
    {
        return TimeToLive != Int64.MaxValue;
    }
}