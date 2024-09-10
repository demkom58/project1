using System;
using System.Collections;
using System.Collections.Generic;
using Godot;

namespace project1.addons.sbgoap.ai.memory;

[Tool]
public partial class Memories : Node
{
    private readonly Dictionary<MemoryModuleType<object>, ExpirableValue<object>?> _memories = new();
    
    public override string[] _GetConfigurationWarnings()
    {
        if (GetParent() is not Brain) return new[] { "Node must be a child of a Brain node." };
        return base._GetConfigurationWarnings();
    }

    public override void _EnterTree()
    {
        if (GetParent() is not Brain brain) return;
        brain.Memories = this;
    }

    public override void _ExitTree()
    {
        if (GetParent() is not Brain brain) return;
        brain.Memories = null;
    }
    
    public void ClearMemories()
    {
        foreach (var memoryModuleType in _memories.Keys) _memories[memoryModuleType] = new ExpirableValue<object>();
    }
    
    public bool HasMemoryValue<T>(MemoryModuleType<T> memoryModuleType)
    {
        return CheckMemory(memoryModuleType, MemoryStatus.ValuePresent);
    }
    
    public void EraseMemory<T>(MemoryModuleType<T> memoryModuleType)
    {
        SetMemoryInternal(memoryModuleType, null);
    }

    public void SetMemory<T>(MemoryModuleType<T> memoryModuleType, T value)
    {
        SetMemoryInternal(memoryModuleType, new ExpirableValue<T>(value));
    }

    public void SetMemoryWithExpiry<T>(MemoryModuleType<T> memoryModuleType, T value, long ttl)
    {
        SetMemoryInternal(memoryModuleType, new ExpirableValue<T>(value, ttl));
    }

    private void SetMemoryInternal<T>(MemoryModuleType<T> memoryModuleType, ExpirableValue<T>? value)
    {
        var key = (MemoryModuleType<object>)(object)memoryModuleType;

        if (!_memories.ContainsKey(key)) return;

        if (value.HasValue && IsEmptyCollection(value.Value.Value!))
            EraseMemory(memoryModuleType);
        else
            _memories[key] = (ExpirableValue<object>?)(object?)value;
    }

    public T? GetMemory<T>(MemoryModuleType<T> memoryModuleType)
    {
        var key = (MemoryModuleType<object>)(object)memoryModuleType;

#pragma warning disable CA1854
        if (!_memories.ContainsKey(key))
            throw new InvalidOperationException("Unregistered memory fetched: " + memoryModuleType);
#pragma warning restore CA1854

        var stored = _memories[key];
        return stored.HasValue ? (T?)stored.Value.Value : default;
    }

    public T? GetMemoryInternal<T>(MemoryModuleType<T> memoryModuleType)
    {
        var key = (MemoryModuleType<object>)(object)memoryModuleType;
        var stored = _memories[key];
        return stored.HasValue ? (T?)stored.Value.Value : default;
    }

    public long GetTimeUntilExpiry<T>(MemoryModuleType<T> memoryModuleType)
    {
        var key = (MemoryModuleType<object>)(object)memoryModuleType;
        var stored = _memories[key];
        return stored?.TimeToLive ?? 0L;
    }

    public bool IsMemoryValue<T>(MemoryModuleType<T> memoryModuleType, T value)
    {
        return HasMemoryValue(memoryModuleType) && GetMemory(memoryModuleType)!.Equals(value);
    }

    public bool CheckMemory<T>(MemoryModuleType<T> memoryModuleType, MemoryStatus memoryStatus)
    {
        var key = (MemoryModuleType<object>)(object)memoryModuleType;

#pragma warning disable CA1854
        if (!_memories.ContainsKey(key)) return false;
#pragma warning restore CA1854

        var memory = _memories[key];
        return memoryStatus switch
        {
            MemoryStatus.Registered => true,
            MemoryStatus.ValuePresent => memory.HasValue,
            MemoryStatus.ValueAbsent => !memory.HasValue,
            _ => false
        };
    }
    
    private void ForgetOutdatedMemories()
    {
        foreach (var (key, expirableValue) in _memories)
        {
            if (!expirableValue.HasValue) continue;
            
            if (expirableValue.Value.IsExpired) EraseMemory(key);
            else expirableValue.Value.Update();
        }
    }
    
    private static bool IsEmptyCollection(object obj)
    {
        return obj is ICollection { Count: 0 };
    }

    public readonly record struct MemoryValue<TU>(MemoryModuleType<TU> Type, ExpirableValue<TU>? Value)
    {
        public void SetMemoryInternal(Memories memory) => memory.SetMemoryInternal(Type, Value);
    }
}