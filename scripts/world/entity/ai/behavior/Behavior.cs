using System;
using System.Collections.Generic;
using Godot;
using project1.scripts.world.entity.ai.memory;

/*
public abstract class Behavior<E extends LivingEntity>
implements BehaviorControl<E> {
    public static final int DEFAULT_DURATION = 60;
    protected final Map<MemoryModuleType<?>, MemoryStatus> entryCondition;
    private Status status = Status.STOPPED;
    private long endTimestamp;
    private final int minDuration;
    private final int maxDuration;

    public Behavior(Map<MemoryModuleType<?>, MemoryStatus> map) {
        this(map, 60);
    }

    public Behavior(Map<MemoryModuleType<?>, MemoryStatus> map, int i) {
        this(map, i, i);
    }

    public Behavior(Map<MemoryModuleType<?>, MemoryStatus> map, int i, int j) {
        this.minDuration = i;
        this.maxDuration = j;
        this.entryCondition = map;
    }

    @Override
    public Status getStatus() {
        return this.status;
    }

    @Override
    public final boolean tryStart(ServerLevel serverLevel, E livingEntity, long l) {
        if (this.hasRequiredMemories(livingEntity) && this.checkExtraStartConditions(serverLevel, livingEntity)) {
            this.status = Status.RUNNING;
            int i = this.minDuration + serverLevel.getRandom().nextInt(this.maxDuration + 1 - this.minDuration);
            this.endTimestamp = l + (long)i;
            this.start(serverLevel, livingEntity, l);
            return true;
        }
        return false;
    }

    protected void start(ServerLevel serverLevel, E livingEntity, long l) {
    }

    @Override
    public final void tickOrStop(ServerLevel serverLevel, E livingEntity, long l) {
        if (!this.timedOut(l) && this.canStillUse(serverLevel, livingEntity, l)) {
            this.tick(serverLevel, livingEntity, l);
        } else {
            this.doStop(serverLevel, livingEntity, l);
        }
    }

    protected void tick(ServerLevel serverLevel, E livingEntity, long l) {
    }

    @Override
    public final void doStop(ServerLevel serverLevel, E livingEntity, long l) {
        this.status = Status.STOPPED;
        this.stop(serverLevel, livingEntity, l);
    }

    protected void stop(ServerLevel serverLevel, E livingEntity, long l) {
    }

    protected boolean canStillUse(ServerLevel serverLevel, E livingEntity, long l) {
        return false;
    }

    protected boolean timedOut(long l) {
        return l > this.endTimestamp;
    }

    protected boolean checkExtraStartConditions(ServerLevel serverLevel, E livingEntity) {
        return true;
    }

    protected boolean hasRequiredMemories(E livingEntity) {
        for (Map.Entry<MemoryModuleType<?>, MemoryStatus> entry : this.entryCondition.entrySet()) {
            MemoryModuleType<?> memoryModuleType = entry.getKey();
            MemoryStatus memoryStatus = entry.getValue();
            if (((LivingEntity)livingEntity).getBrain().checkMemory(memoryModuleType, memoryStatus)) continue;
            return false;
        }
        return true;
    }
}
*/
namespace project1.scripts.world.entity.ai.behavior;

public class Behavior<T> : IBehaviorControl<T> where T : ILivingEntity
{
    public const int DEFAULT_DURATION = 60;
    
    protected readonly Dictionary<MemoryModuleType<object>, MemoryStatus> EntryCondition;
    private long _endTimestamp;
    private readonly int _minDuration;
    private readonly int _maxDuration;
    public BehaviorStatus Status { get; private set;} = BehaviorStatus.Stopped;

    public Behavior(
        Dictionary<MemoryModuleType<object>, MemoryStatus> entryCondition, 
        int minDuration = DEFAULT_DURATION, 
        int maxDuration = DEFAULT_DURATION)
    {
        this.EntryCondition = entryCondition;
        this._minDuration = minDuration;
        this._maxDuration = maxDuration;
    }

    public bool TryStart(Node level, T entity, long updateNumber)
    {
        if (this.HasRequiredMemories(entity) && this.CheckExtraStartConditions(level, entity))
        {
            this.Status = BehaviorStatus.Running;

            int randomAddition = new Random().Next(this._maxDuration + 1 - this._minDuration);
            int randomizedDuration = this._minDuration + randomAddition;
            this._endTimestamp = updateNumber + randomizedDuration;
            
            this.Start(level, entity, updateNumber);
            return true;
        }
        return false;
    }

    protected void Start(Node level, T entity, long updateNumber)
    {
    }

    public void TickOrStop(Node level, T entity, long updateNumber)
    {
        if (!this.TimedOut(updateNumber) && this.CanStillUse(level, entity, updateNumber))
        {
            this.Tick(level, entity, updateNumber);
        }
        else
        {
            this.DoStop(level, entity, updateNumber);
        }
    }

    protected void Tick(Node level, T entity, long updateNumber)
    {
    }

    public void DoStop(Node level, T entity, long updateNumber)
    {
        this.Status = BehaviorStatus.Stopped;
        this.Stop(level, entity, updateNumber);
    }

    protected void Stop(Node level, T entity, long updateNumber)
    {
    }

    protected bool CanStillUse(Node level, T entity, long updateNumber)
    {
        return false;
    }
    
    protected bool TimedOut(long updateNumber)
    {
        return updateNumber > this._endTimestamp;
    }
    
    protected bool CheckExtraStartConditions(Node level, T entity)
    {
        return true;
    }
    
    protected bool HasRequiredMemories(T entity)
    {
        foreach (KeyValuePair<MemoryModuleType<object>, MemoryStatus> entry in this.EntryCondition)
        {
            if (entity.Brain.CheckMemory(entry.Key, entry.Value)) continue;
            return false;
        }
        return true;
    }
}