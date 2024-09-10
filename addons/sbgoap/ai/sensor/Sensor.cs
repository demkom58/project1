using System;
using System.Collections.Generic;
using Godot;
using project1.addons.sbgoap.ai.memory;

namespace project1.addons.sbgoap.ai.sensor;

public abstract partial class Sensor : Node
{
    public const int DefaultScanRate = 20;

    public static readonly Random Random = new();
    
    [Export]
    public int ScanRate = DefaultScanRate;
    private long _timeToTick = 0;

    public override void _Ready()
    {
        _timeToTick = Random.Next(DefaultScanRate);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (--_timeToTick > 0) return;
        _timeToTick = ScanRate;
        Scan();
    }

    protected abstract void Scan();

    public abstract HashSet<MemoryModuleType<object>> Requires();
}