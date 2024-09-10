using System.Collections.Generic;
using Godot;

namespace project1.addons.sbgoap.ai.memory;

[Tool]
[GlobalClass]
public partial class MemoryNode : Node
{
    [Export]
    public GodotObject? Value { get; set; }
    public string ObjectType => Value?.GetClass() ?? "null";
    
    public MemoryNode() : this(null) { }

    public MemoryNode(GodotObject? value)
    {
        Value = value;
    }
    
    public virtual void Clear()
    {
        Value = null;
    }
    
    public override string[] _GetConfigurationWarnings()
    {
        if (GetParent() is not Memories) return new[] { "Node must be a child of a Memories node." };
        return base._GetConfigurationWarnings();
    }

    public override void _EnterTree()
    {
        if (GetParent() is not Memories memories) return;
        memories.Content[GetName()] = this;
    }
    
    public override void _ExitTree()
    {
        if (GetParent() is not Memories memories) return;
        memories.Content.Remove(GetName());
    }
}