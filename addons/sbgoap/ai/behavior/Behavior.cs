using System;
using System.Collections.Generic;
using Godot;
using project1.addons.sbgoap.ai.memory;

namespace project1.addons.sbgoap.ai.behavior;

public class Behavior : IBehaviorControl
{
    public const int DEFAULT_DURATION = 60;
    private readonly int _maxDuration;
    private readonly int _minDuration;

    protected readonly Dictionary<MemoryModuleType<object>, MemoryStatus> EntryCondition;
    private long _endTimestamp;

    public Behavior(
        Dictionary<MemoryModuleType<object>, MemoryStatus> entryCondition,
        int minDuration = DEFAULT_DURATION,
        int maxDuration = DEFAULT_DURATION)
    {
        EntryCondition = entryCondition;
        _minDuration = minDuration;
        _maxDuration = maxDuration;
    }

    public BehaviorStatus Status { get; private set; } = BehaviorStatus.Stopped;

    public bool TryStart(long gameTime)
    {
        if (HasRequiredMemories() && CheckExtraStartConditions())
        {
            Status = BehaviorStatus.Running;

            var randomAddition = new Random().Next(_maxDuration + 1 - _minDuration);
            var randomizedDuration = _minDuration + randomAddition;
            _endTimestamp = gameTime + randomizedDuration;

            Start(gameTime);
            return true;
        }

        return false;
    }

    public void UpdateOrStop(long gameTime)
    {
        if (!TimedOut(gameTime) && CanStillUse(gameTime))
            Tick(gameTime);
        else
            DoStop(gameTime);
    }

    public void DoStop(long gameTime)
    {
        Status = BehaviorStatus.Stopped;
        Stop(gameTime);
    }

    protected void Start(long gameTime)
    {
    }

    protected void Tick(long gameTime)
    {
    }

    protected void Stop(long gameTime)
    {
    }

    protected bool CanStillUse(long gameTime)
    {
        return false;
    }

    protected bool TimedOut(long updateNumber)
    {
        return updateNumber > _endTimestamp;
    }

    protected bool CheckExtraStartConditions()
    {
        return true;
    }

    protected bool HasRequiredMemories()
    {
        foreach (var entry in EntryCondition)
        {
            // todo: if (entity.Brain.CheckMemory(entry.Key, entry.Value)) continue;
            return false;
        }

        return true;
    }
}