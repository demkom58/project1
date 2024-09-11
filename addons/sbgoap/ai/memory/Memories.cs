using System.Collections.Generic;
using System.Collections.ObjectModel;
using Godot;

namespace project1.addons.sbgoap.ai.memory;

[Tool]
public partial class Memories : Node
{
    private readonly Godot.Collections.Dictionary<string, MemoryNode> _content = new();
    public ReadOnlyDictionary<string, MemoryNode> Content => _content.AsReadOnly();
    
    public Memories()
    {
        ChildEnteredTree += node =>
        {
            if (node is MemoryNode memory) _content[memory.Name] = memory;
        };
        
        ChildExitingTree += node =>
        {
            if (node is MemoryNode memory) _content.Remove(memory.Name);
        };
    }
    
    public override string[] _GetConfigurationWarnings()
    {
        if (GetParent() is not Brain) return new[] { "Node must be a child of a Brain node." };
        return base._GetConfigurationWarnings();
    }
    
    public void ClearMemories()
    {
        foreach (var node in _content.Values)
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
        if (_content.TryGetValue(memoryName, out var memory))
        {
            memory.Value = null;
            return;
        }
        
        GD.PushError($"Attempt to erase unregistered memory: {memoryName}");
    }

    public void SetMemory(string memoryName, GodotObject? value)
    {
        if (_content.TryGetValue(memoryName, out var memory))
        {
            memory.Value = value;
            return;
        }
   
        GD.PushError($"Attempt to set unregistered memory: {memoryName}");
    }

    public GodotObject? GetMemory(string memoryName)
    {
        if (_content.TryGetValue(memoryName, out var value)) return value.Value;
        
        GD.PushError($"Attempt to access unregistered memory: {memoryName}");
        return null;
    }

    public long? GetTimeUntilExpiry(string memoryName)
    {
        var stored = _content[memoryName];
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
        if (!_content.TryGetValue(memoryName, out var memory)) 
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