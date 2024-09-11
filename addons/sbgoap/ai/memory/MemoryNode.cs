using Godot;

namespace project1.addons.sbgoap.ai.memory;

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
}