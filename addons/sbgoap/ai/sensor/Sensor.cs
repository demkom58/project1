using System;
using System.Collections.Generic;
using Godot;
using project1.addons.sbgoap.ai.memory;

namespace project1.addons.sbgoap.ai.sensor;

[Tool]
public abstract partial class Sensor : Node
{
    public const int DefaultScanRate = 20;

    public static readonly Random Random = new();
    
    [Export]
    public int ScanRate = DefaultScanRate;
    private long _timeToTick = 0;

    public override string[] _GetConfigurationWarnings()
    {
        if (GetParent() is not Sensors) return new[] { "Node must be a child of a Sensors node." };
        return base._GetConfigurationWarnings();
    }

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

    /// <summary>
    /// Scans the environment for information.
    /// </summary>
    protected abstract void Scan();

    /// <summary>
    /// Names of memory nodes that this sensor requires to function.
    /// </summary>
    /// <returns>A set of memory node names.</returns>
    public abstract HashSet<string> Requires();
}