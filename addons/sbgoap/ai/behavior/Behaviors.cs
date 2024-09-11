using Godot;

namespace project1.addons.sbgoap.ai.behavior;

[Tool]
[GlobalClass]
public partial class Behaviors : Node
{
    public override string[] _GetConfigurationWarnings()
    {
        if (GetParent() is not Brain) return new[] { "Node must be a child of a Brain node." };
        return base._GetConfigurationWarnings();
    }
}