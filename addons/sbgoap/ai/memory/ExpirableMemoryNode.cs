using Godot;

namespace project1.addons.sbgoap.ai.memory;

[GlobalClass]
public partial class ExpirableMemoryNode : MemoryNode
{
    [Export] public int TimeToLive { get; set; }
    public bool IsExpired => TimeToLive <= 0;
    public bool IsExpirable => TimeToLive != int.MaxValue;
    
    public ExpirableMemoryNode() : this(null) { }

    public ExpirableMemoryNode(GodotObject? value, int timeToLive = int.MaxValue) : base(value)
    {
        SetPhysicsProcess(true);
        TimeToLive = timeToLive;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Value == null) return;
        
        if (IsExpirable && !IsExpired) TimeToLive--;
        else if (IsExpired) Clear();
    }

    public override void Clear()
    {
        Value = null;
        TimeToLive = 0;
    }
}