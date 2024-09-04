using System;
using System.Collections.Generic;
using project1.scripts.world.entity.ai.memory;

namespace project1.scripts.world.entity.ai.behavior;

public class Behavior<T> : IBehaviorControl<T> where T : ILivingEntity
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

    public bool TryStart(IWorld level, T entity, long gameTime)
    {
        if (HasRequiredMemories(entity) && CheckExtraStartConditions(level, entity))
        {
            Status = BehaviorStatus.Running;

            var randomAddition = new Random().Next(_maxDuration + 1 - _minDuration);
            var randomizedDuration = _minDuration + randomAddition;
            _endTimestamp = gameTime + randomizedDuration;

            Start(level, entity, gameTime);
            return true;
        }

        return false;
    }

    public void UpdateOrStop(IWorld level, T entity, long gameTime)
    {
        if (!TimedOut(gameTime) && CanStillUse(level, entity, gameTime))
            Tick(level, entity, gameTime);
        else
            DoStop(level, entity, gameTime);
    }

    public void DoStop(IWorld level, T entity, long gameTime)
    {
        Status = BehaviorStatus.Stopped;
        Stop(level, entity, gameTime);
    }

    protected void Start(IWorld level, T entity, long gameTime)
    {
    }

    protected void Tick(IWorld level, T entity, long gameTime)
    {
    }

    protected void Stop(IWorld level, T entity, long gameTime)
    {
    }

    protected bool CanStillUse(IWorld level, T entity, long gameTime)
    {
        return false;
    }

    protected bool TimedOut(long updateNumber)
    {
        return updateNumber > _endTimestamp;
    }

    protected bool CheckExtraStartConditions(IWorld level, T entity)
    {
        return true;
    }

    protected bool HasRequiredMemories(T entity)
    {
        foreach (var entry in EntryCondition)
        {
            if (entity.Brain.CheckMemory(entry.Key, entry.Value)) continue;
            return false;
        }

        return true;
    }
}