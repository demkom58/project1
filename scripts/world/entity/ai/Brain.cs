using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using project1.scripts.world.entity.ai.behavior;
using project1.scripts.world.entity.ai.memory;
using project1.scripts.world.entity.ai.schedule;
using project1.scripts.world.entity.ai.sensor;

namespace project1.scripts.world.entity.ai;

public partial class Brain<TOwner> : Node where TOwner : ILivingEntity
{
    private Dictionary<MemoryModuleType<object>, ExpirableValue<object>> _memories = new ();
    private Dictionary<SensorType<Sensor<ILivingEntity>,ILivingEntity>, Sensor<ILivingEntity>> _sensors = new ();
    private SortedDictionary<int, Dictionary<Activity, HashSet<Behavior<TOwner>>>> _availableBehaviorsByPriority = new ();

    private Dictionary<Activity, HashSet<Tuple<MemoryModuleType<object>, MemoryStatus>>> _activityRequirements = new ();
    private Dictionary<Activity, HashSet<MemoryModuleType<object>>> _activityMemoriesToEraseWhenStopped = new ();
    private long _lastScheduleUpdate = 0L;

    public Schedule Schedule { get; set; } = Schedule.Empty;
    public HashSet<Activity> CoreActivities { get; set; }= new ();
    public HashSet<Activity> ActiveActivities { get; } = new ();
    public Activity DefaultActivity { get; set; } = Activity.Idle;
    
    public Brain(
        ICollection<MemoryModuleType<object>> memoryTypes, 
        ICollection<SensorType<Sensor<ILivingEntity>,ILivingEntity>> sensors,
        ICollection<MemoryValue<object>> memories)
    {
        foreach (MemoryModuleType<object> type in memoryTypes)
        {
            _memories.Add(type, new ExpirableValue<object>());
        }
        
        foreach (SensorType<Sensor<ILivingEntity>,ILivingEntity> sensorType in sensors)
        {
            _sensors.Add(sensorType, sensorType.Create());
        }
        
        foreach (Sensor<ILivingEntity> sensor in _sensors.Values)
        {
            foreach (MemoryModuleType<object> required in sensor.Requires())
            {
                _memories.Add(required, new ExpirableValue<object>());
            }
        }
        
        foreach (MemoryValue<object> memoryValue in memories)
        {
            memoryValue.SetMemoryInternal(this);
        }
    }
    
    public bool hasMemoryValue(MemoryModuleType<object> memoryModuleType)
    {
        return checkMemory(memoryModuleType, MemoryStatus.ValuePresent);
    }
    
    public void clearMemories()
    {
        foreach (MemoryModuleType<object> memoryModuleType in _memories.Keys)
        {
            _memories[memoryModuleType] = new ExpirableValue<object>();
        }
    }
    
    public void eraseMemory<T>(MemoryModuleType<T> memoryModuleType)
    {
        setMemory(memoryModuleType, Optional<T>.Empty);
    }
    
    public void setMemory(MemoryModuleType<TOwner> memoryModuleType, TOwner value)
    {
        setMemory(memoryModuleType, Optional<TOwner>.Of(value));
    }
    
    public void setMemoryWithExpiry<T>(MemoryModuleType<T> memoryModuleType, T value, long ttl)
    {
        setMemoryInternal(memoryModuleType, Optional<ExpirableValue<T>>.Of(ExpirableValue<T>.Of(value, ttl)));
    }
    
    public void setMemory<T>(MemoryModuleType<T> memoryModuleType, Optional<T> optional)
    {
        setMemoryInternal(memoryModuleType, optional.Map(ExpirableValue<T>.Of));
    }
    
    private void setMemoryInternal<T>(MemoryModuleType<T> memoryModuleType, Optional<ExpirableValue<T>> optional)
    {
        if (_memories.ContainsKey(memoryModuleType))
        {
            if (optional.HasValue && IsEmptyCollection(optional.Value.Value))
            {
                eraseMemory(memoryModuleType);
            }
            else
            {
                _memories[memoryModuleType] = optional.Value;
            }
        }
    }
    
    public Optional<T> getMemory<T>(MemoryModuleType<T> memoryModuleType)
    {
        Optional<ExpirableValue<T>> optional = _memories[memoryModuleType];
        if (optional == null)
        {
            throw new InvalidOperationException("Unregistered memory fetched: " + memoryModuleType);
        }
        return optional.Map(ExpirableValue<T>.GetValue);
    }
    
    public Optional<T> getMemoryInternal<T>(MemoryModuleType<T> memoryModuleType)
    {
        Optional<ExpirableValue<T>> optional = _memories[memoryModuleType];
        return optional.Map(ExpirableValue<T>.GetValue);
    }
    
    public long getTimeUntilExpiry<T>(MemoryModuleType<T> memoryModuleType)
    {
        Optional<ExpirableValue<T>> optional = _memories[memoryModuleType];
        return optional.Map(ExpirableValue<T>.GetTimeToLive).OrElse(0L);
    }
    
    public bool isMemoryValue<T>(MemoryModuleType<T> memoryModuleType, T value)
    {
        return hasMemoryValue(memoryModuleType) 
               && getMemory(memoryModuleType).Filter(o => o.Equals(value)).HasValue;
    }
    
    public bool checkMemory<T>(MemoryModuleType<T> memoryModuleType, MemoryStatus memoryStatus)
    {
        Optional<ExpirableValue<T>> optional = _memories[memoryModuleType];
        return memoryStatus switch
        {
            MemoryStatus.Registered => true,
            MemoryStatus.ValuePresent => optional.HasValue,
            MemoryStatus.ValueAbsent => !optional.HasValue,
            _ => false
        };
    }
    
    public List<Behavior<TOwner>> GetRunningBehaviors()
    {
        return (
            from map in _availableBehaviorsByPriority.Values 
            from set in map.Values 
            from behavior in set 
            where behavior.Status == BehaviorStatus.Running 
            select behavior
            ).ToList();
    }
    
    public void UseDefaultActivity()
    {
        SetActiveActivity(DefaultActivity);
    }
    
    public Optional<Activity> GetActiveNonCoreActivity()
    {
        foreach (Activity activity in ActiveActivities)
        {
            if (!CoreActivities.Contains(activity))
            {
                return Optional<Activity>.Of(activity);
            }
        }
        return Optional<Activity>.Empty;
    }
    
    public void SetActiveActivityIfPossible(Activity activity)
    {
        if (activityRequirementsAreMet(activity))
        {
            SetActiveActivity(activity);
        }
        else
        {
            UseDefaultActivity();
        }
    }
    
    private void SetActiveActivity(Activity activity)
    {
        if (IsActive(activity)) return;
        
        EraseMemoriesForOtherActivitiesThan(activity);
        ActiveActivities.Clear();
        ActiveActivities.UnionWith(CoreActivities);
        ActiveActivities.Add(activity);
    }
    
    private void EraseMemoriesForOtherActivitiesThan(Activity activity)
    {
        foreach (Activity activeActivity in ActiveActivities)
        {
            if (activeActivity.Equals(activity)) continue;
            if (!_activityMemoriesToEraseWhenStopped.TryGetValue(activeActivity, out var value)) continue;
            
            foreach (MemoryModuleType<object> memoryModuleType in value)
            {
                eraseMemory(memoryModuleType);
            }
        }
    }
    
    public void UpdateActivityFromSchedule(long dayTime, long gameTime)
    {
        if (gameTime - _lastScheduleUpdate <= 20L) return;
        
        _lastScheduleUpdate = gameTime;
        Activity activity = Schedule.GetActivityAt((int)(dayTime % 24000L));
        if (!ActiveActivities.Contains(activity))
        {
            SetActiveActivityIfPossible(activity);
        }
    }
    
    public void SetActiveActivityToFirstValid(List<Activity> list)
    {
        foreach (Activity activity in list)
        {
            if (activityRequirementsAreMet(activity))
            {
                SetActiveActivity(activity);
                break;
            }
        }
    }
    
    public void AddActivity(Activity activity, int i, ImmutableList<Behavior<TOwner>> immutableList)
    {
        AddActivity(activity, CreatePriorityTuples(i, immutableList));
    }
    
    public void AddActivityAndRemoveMemoryWhenStopped(
        Activity activity, 
        int priority, 
        IEnumerable<Behavior<TOwner>> behaviors, 
        MemoryModuleType<object> memoryType)
    {
        HashSet<Tuple<MemoryModuleType<object>, MemoryStatus>> memoriesRequirements 
            = new (){new Tuple<MemoryModuleType<object>, MemoryStatus>(memoryType, MemoryStatus.ValuePresent)};
        
        HashSet<MemoryModuleType<object>> eraseOnStopMemories = new (){ memoryType };
        
        AddActivityAndRemoveMemoriesWhenStopped(activity, 
            CreatePriorityTuples(priority, behaviors), memoriesRequirements, eraseOnStopMemories);
    }
    
    public void AddActivity(Activity activity, IEnumerable<Tuple<int, Behavior<TOwner>>> behaviors)
    {
        AddActivityAndRemoveMemoriesWhenStopped(activity, behaviors, new (), new ());
    }
    
    public void AddActivityWithConditions(
        Activity activity, 
        IEnumerable<Tuple<int, Behavior<TOwner>>> behaviors, 
        HashSet<Tuple<MemoryModuleType<object>, MemoryStatus>> set)
    {
        AddActivityAndRemoveMemoriesWhenStopped(activity, behaviors, set, new ());
    }
    
    public void AddActivityAndRemoveMemoriesWhenStopped(
        Activity act, 
        IEnumerable<Tuple<int, Behavior<TOwner>>> behaviors, 
        HashSet<Tuple<MemoryModuleType<object>, MemoryStatus>> memoriesRequirements, 
        HashSet<MemoryModuleType<object>> eraseOnStopMemories)
    {
        _activityRequirements.Add(act, memoriesRequirements);
        if (eraseOnStopMemories.Count > 0)
        {
            _activityMemoriesToEraseWhenStopped.Add(act, eraseOnStopMemories);
        }
        foreach (Tuple<int, Behavior<TOwner>> tuple in behaviors)
        {
            var activity2Behaviors = _availableBehaviorsByPriority[tuple.Item1];
            if (activity2Behaviors == null)
            {
                activity2Behaviors = new Dictionary<Activity, HashSet<Behavior<TOwner>>>();
                _availableBehaviorsByPriority[tuple.Item1] = activity2Behaviors;
            }
            
            var behs = activity2Behaviors[act];
            if (behs == null)
            {
                behs = new HashSet<Behavior<TOwner>>();
                activity2Behaviors[act] = behs;
            }
            
            behs.Add(tuple.Item2);
        }
    }
    
    public void RemoveAllBehaviors()
    {
        _availableBehaviorsByPriority.Clear();
    }
    
    public bool IsActive(Activity activity)
    {
        return ActiveActivities.Contains(activity);
    }
    
    public Brain<TOwner> CopyWithoutBehaviors()
    {
        Brain<TOwner> brain = new Brain<TOwner>(_memories.Keys, _sensors.Keys, ImmutableList<MemoryValue<object>>.Empty);
        foreach (KeyValuePair<MemoryModuleType<object>, ExpirableValue<object>> entry in _memories)
        {
            MemoryModuleType<object> memoryModuleType = entry.Key;
            if (entry.Value.HasValue)
            {
                brain._memories[memoryModuleType] = entry.Value;
            }
        }
        return brain;
    }
    
    public void Tick(ServerLevel serverLevel, TOwner livingEntity)
    {
        ForgetOutdatedMemories();
        TickSensors(serverLevel, livingEntity);
        StartEachNonRunningBehavior(serverLevel, livingEntity);
        TickEachRunningBehavior(serverLevel, livingEntity);
    }
    
    private void TickSensors(ServerLevel serverLevel, TOwner livingEntity)
    {
        foreach (Sensor<ILivingEntity> sensor in _sensors.Values)
        {
            sensor.Tick(serverLevel, livingEntity);
        }
    }
    
    private void ForgetOutdatedMemories()
    {
        foreach (KeyValueTuple<MemoryModuleType<object>, ExpirableValue<object>> entry in _memories)
        {
            if (!entry.Value.HasValue) continue;
            
            ExpirableValue<object> expirableValue = entry.Value;
            if (expirableValue.HasExpired())
            {
                eraseMemory(entry.Key);
            }
            expirableValue.Update();
        }
    }
    
    public void StopAll(ServerLevel serverLevel, TOwner livingEntity)
    {
        long gameTime = ((Entity)livingEntity).Level.GetGameTime();
        foreach (Behavior<TOwner> behavior in GetRunningBehaviors())
        {
            behavior.DoStop(serverLevel, livingEntity, gameTime);
        }
    }
    
    private void StartEachNonRunningBehavior(ServerLevel serverLevel, TOwner livingEntity)
    {
        long gameTime = serverLevel.GetGameTime();
        foreach (Dictionary<Activity, HashSet<Behavior<TOwner>>> map in _availableBehaviorsByPriority.Values)
        {
            foreach (KeyValueTuple<Activity, HashSet<Behavior<TOwner>>> entry in map)
            {
                Activity activity = entry.Key;
                if (ActiveActivities.Contains(activity))
                {
                    continue;
                }
                HashSet<Behavior<TOwner>> set = entry.Value;
                foreach (Behavior<TOwner> behavior in set)
                {
                    if (behavior.Status != BehaviorStatus.Stopped)
                    {
                        continue;
                    }
                    behavior.TryStart(serverLevel, livingEntity, gameTime);
                }
            }
        }
    }
    
    private void TickEachRunningBehavior(ServerLevel serverLevel, TOwner livingEntity)
    {
        long gameTime = serverLevel.GetGameTime();
        foreach (Behavior<TOwner> behavior in GetRunningBehaviors())
        {
            behavior.TickOrStop(serverLevel, livingEntity, gameTime);
        }
    }
    
    private bool activityRequirementsAreMet(Activity activity)
    {
        if (!_activityRequirements.ContainsKey(activity))
        {
            return false;
        }
        foreach (Tuple<MemoryModuleType<object>, MemoryStatus> Tuple in _activityRequirements[activity])
        {
            MemoryStatus memoryStatus = Tuple.Item2;
            MemoryModuleType<object> memoryModuleType = Tuple.Item1;
            if (!checkMemory(memoryModuleType, memoryStatus))
            {
                return false;
            }
        }
        return true;
    }
    
    private static bool IsEmptyCollection(object obj)
    {
        return obj is ICollection { Count: 0 };
    }
    
    private List<Tuple<int, Behavior<TOwner>>> CreatePriorityTuples(int priority, IEnumerable<Behavior<TOwner>> behaviors)
    {
        int priorityCounter = priority;
        return behaviors.Select(behavior => new Tuple<int, Behavior<TOwner>>(priorityCounter++, behavior)).ToList();
    }
    
    
    public readonly struct MemoryValue<TU>
    {
        private readonly MemoryModuleType<TU> _type;
        private readonly Optional<ExpirableValue<TU>> _value;

        public MemoryValue(MemoryModuleType<TU> type, Optional<ExpirableValue<TU>> value)
        {
            _type = type;
            _value = value;
        }

        public void SetMemoryInternal(Brain<TOwner> brain)
        {
            brain.setMemoryInternal(_type, _value);
        }
    }
}