using System.Collections.Generic;
using System.Collections.ObjectModel;
using Godot;

namespace project1.addons.sbgoap.ai.sensor;

[Tool]
public partial class Sensors : Node
{
    private readonly List<Sensor> _content = new();
    public ReadOnlyCollection<Sensor> Content => _content.AsReadOnly();

    public Sensors()
    {
        ChildEnteredTree += node =>
        {
            if (node is Sensor sensor) _content.Add(sensor);
        };
        
        ChildExitingTree += node =>
        {
            if (node is Sensor sensor) _content.Remove(sensor);
        };
    }
    
    public override string[] _GetConfigurationWarnings()
    {
        if (GetParent() is not Brain) return new[] { "Node must be a child of a Brain node." };
        return base._GetConfigurationWarnings();
    }
}