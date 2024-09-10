using System.Collections.Generic;
using Godot;

namespace project1.addons.sbgoap.ai.sensor;

[Tool]
public partial class Sensors : Node
{
    private readonly List<Sensor> _sensors = new();
    
    public override string[] _GetConfigurationWarnings()
    {
        if (GetParent() is not Brain) return new[] { "Node must be a child of a Brain node." };
        return base._GetConfigurationWarnings();
    }

    public override void _EnterTree()
    {
        if (GetParent() is not Brain brain) return;
        brain.Sensors = this;
    }

    public override void _ExitTree()
    {
        if (GetParent() is not Brain brain) return;
        brain.Sensors = null;
    }
}