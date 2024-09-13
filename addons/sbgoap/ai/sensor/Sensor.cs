using System;
using System.Collections.Generic;
using project1.addons.sbgoap.ai.memory;

namespace project1.addons.sbgoap.ai.sensor;

public abstract class Sensor
{
    public static readonly Random Random = new();

    private readonly int _scanRate;
    private long _timeToTick;

    public Sensor(int scanRate = 20)
    {
        _scanRate = scanRate;
        _timeToTick = Random.Next(scanRate);
    }

    public void Update(Brain brain)
    {
        if (--_timeToTick > 0) return;
        _timeToTick = _scanRate;
        PostUpdate(brain);
    }

    protected abstract void PostUpdate(Brain brain);

    public abstract HashSet<MemoryModuleType<object>> Requires();
}