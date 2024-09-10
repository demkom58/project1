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

    public override void _EnterTree()
    {
        if (GetParent() is not Brain brain) return;
        brain.Behaviors = this;
    }

    public override void _ExitTree()
    {
        if (GetParent() is not Brain brain) return;
        brain.Behaviors = null;
    }
}