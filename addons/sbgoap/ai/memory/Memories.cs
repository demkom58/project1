using Godot;

namespace project1.addons.sbgoap.ai.memory;

[Tool]
public partial class Memories : Node
{
    public readonly Godot.Collections.Dictionary<string, MemoryNode> Content = new();
    
    public Memories()
    {
        ChildEnteredTree += node =>
        {
            if (node is MemoryNode memory) Content[memory.Name] = memory;
        };
        
        ChildExitingTree += node =>
        {
            if (node is MemoryNode memory) Content.Remove(memory.Name);
        };
    }
    
    public override string[] _GetConfigurationWarnings()
    {
        if (GetParent() is not Brain) return new[] { "Node must be a child of a Brain node." };
        return base._GetConfigurationWarnings();
    }
    
    public void ClearMemories()
    {
        foreach (var node in Content.Values)
        {
            node.Value = null;
        }
    }
    
    public bool HasMemoryValue(string memoryName)
    {
        return CheckMemory(memoryName, MemoryStatus.ValuePresent);
    }
    
    public void EraseMemory(string memoryName)
    {
        if (Content.TryGetValue(memoryName, out var memory))
        {
            memory.Value = null;
            return;
        }
        
        GD.PushError($"Attempt to erase unregistered memory: {memoryName}");
    }

    public void SetMemory(string memoryName, GodotObject? value)
    {
        if (Content.TryGetValue(memoryName, out var memory))
        {
            memory.Value = value;
            return;
        }
   
        GD.PushError($"Attempt to set unregistered memory: {memoryName}");
    }

    public GodotObject? GetMemory(string memoryName)
    {
        if (Content.TryGetValue(memoryName, out var value)) return value.Value;
        
        GD.PushError($"Attempt to access unregistered memory: {memoryName}");
        return null;
    }

    public long? GetTimeUntilExpiry(string memoryName)
    {
        var stored = Content[memoryName];
        return stored is ExpirableMemoryNode { IsExpirable: true } expirable
            ? expirable.TimeToLive
            : null;
    }

    public bool IsMemoryValue<T>(string memoryName, T value)
    {
        return HasMemoryValue(memoryName) && GetMemory(memoryName)!.Equals(value);
    }

    public bool CheckMemory(string memoryName, MemoryStatus memoryStatus)
    {
        if (!Content.TryGetValue(memoryName, out var memory)) 
            return memoryStatus == MemoryStatus.Unregistered;
        
        return memoryStatus switch
        {
            MemoryStatus.Registered => true,
            MemoryStatus.ValuePresent => memory.Value != null,
            MemoryStatus.ValueAbsent => memory.Value == null,
            _ => false
        };
    }
}