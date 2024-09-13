using System;
using System.Collections.Generic;
using project1.addons.sbgoap.ai.memory;

namespace project1.addons.sbgoap.ai.behavior;

public class Behavior : IBehaviorControl
{
    public const int DefaultDuration = 60;
    private readonly int _maxDuration;
    private readonly int _minDuration;

    protected readonly Dictionary<MemoryModuleType<object>, MemoryStatus> EntryCondition;
    private ulong _endTimestamp;

    public Behavior(
        Dictionary<MemoryModuleType<object>, MemoryStatus> entryCondition,
        int minDuration = DefaultDuration,
        int maxDuration = DefaultDuration)
    {
        EntryCondition = entryCondition;
        _minDuration = minDuration;
        _maxDuration = maxDuration;
    }

    public BehaviorStatus Status { get; private set; } = BehaviorStatus.Stopped;

    public bool TryStart(Brain brain, ulong gameTime)
    {
        if (!HasRequiredMemories(brain) || !CheckExtraStartConditions(brain)) return false;

        Status = BehaviorStatus.Running;

        var randomAddition = new Random().Next(_maxDuration + 1 - _minDuration);
        var randomizedDuration = (ulong)(_minDuration + randomAddition);
        _endTimestamp = gameTime + randomizedDuration;

        Start(brain, gameTime);
        return true;
    }

    public void UpdateOrStop(Brain brain, ulong gameTime)
    {
        if (!TimedOut(gameTime) && CanStillUse(brain, gameTime))
            Tick(brain, gameTime);
        else
            DoStop(brain, gameTime);
    }

    public void DoStop(Brain brain, ulong gameTime)
    {
        Status = BehaviorStatus.Stopped;
        Stop(brain, gameTime);
    }

    protected virtual void Start(Brain brain, ulong gameTime)
    {
    }

    protected virtual void Tick(Brain brain, ulong gameTime)
    {
    }

    protected virtual void Stop(Brain brain, ulong gameTime)
    {
    }

    protected virtual bool CanStillUse(Brain brain, ulong gameTime)
    {
        return false;
    }

    protected virtual bool TimedOut(ulong gameTime)
    {
        return gameTime > _endTimestamp;
    }

    protected virtual bool CheckExtraStartConditions(Brain brain)
    {
        return true;
    }

    protected bool HasRequiredMemories(Brain brain)
    {
        foreach (var entry in EntryCondition)
        {
            if (brain.CheckMemory(entry.Key, entry.Value)) continue;
            return false;
        }

        return true;
    }
}