﻿using Godot;
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

public class Brain<TOwner> where TOwner : ILivingEntity
{
    private readonly Dictionary<MemoryModuleType<object>, ExpirableValue<object>?> _memories = new ();
    private readonly Dictionary<SensorType<Sensor<ILivingEntity>,ILivingEntity>, Sensor<ILivingEntity>> _sensors = new ();
    private readonly SortedDictionary<int, Dictionary<Activity, HashSet<Behavior<TOwner>>>> _availableBehaviorsByPriority = new ();
    private readonly Dictionary<Activity, HashSet<Tuple<MemoryModuleType<object>, MemoryStatus>>> _activityRequirements = new ();
    private readonly Dictionary<Activity, HashSet<MemoryModuleType<object>>> _activityMemoriesToEraseWhenStopped = new ();
    
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
    
    public bool HasMemoryValue<T>(MemoryModuleType<T> memoryModuleType)
    {
        return CheckMemory(memoryModuleType, MemoryStatus.ValuePresent);
    }
    
    public void ClearMemories()
    {
        foreach (MemoryModuleType<object> memoryModuleType in _memories.Keys)
        {
            _memories[memoryModuleType] = new ExpirableValue<object>();
        }
    }
    
    public void EraseMemory<T>(MemoryModuleType<T> memoryModuleType)
    {
        SetMemoryInternal(memoryModuleType, null);
    }
    
    public void SetMemory<T>(MemoryModuleType<T> memoryModuleType, T value)
    {
        SetMemoryInternal(memoryModuleType, new (value));
    }
    
    public void SetMemoryWithExpiry<T>(MemoryModuleType<T> memoryModuleType, T value, long ttl)
    {
        SetMemoryInternal(memoryModuleType, new (value, ttl));
    }
    
    private void SetMemoryInternal<T>(MemoryModuleType<T> memoryModuleType, ExpirableValue<T>? value)
    {
        var key = (MemoryModuleType<object>) (object) memoryModuleType;
        
        if (!_memories.ContainsKey(key)) return;
        
        if (value.HasValue && IsEmptyCollection(value.Value.Value!))
        {
            EraseMemory(memoryModuleType);
        }
        else
        {
            _memories[key] = (ExpirableValue<object>?) (object?) value;
        }
    }
    
    public T? GetMemory<T>(MemoryModuleType<T> memoryModuleType)
    {
        var key = (MemoryModuleType<object>) (object) memoryModuleType;
        // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
        if (!_memories.ContainsKey(key))
        {
            throw new InvalidOperationException("Unregistered memory fetched: " + memoryModuleType);
        }
        
        ExpirableValue<object>? stored = _memories[key];
        return stored.HasValue ? (T?) stored.Value.Value : default;
    }
    
    public T? GetMemoryInternal<T>(MemoryModuleType<T> memoryModuleType)
    {
        var key = (MemoryModuleType<object>) (object) memoryModuleType;
        ExpirableValue<object>? stored = _memories[key];
        return stored.HasValue ? (T?) stored.Value.Value : default;
    }
    
    public long GetTimeUntilExpiry<T>(MemoryModuleType<T> memoryModuleType)
    {
        var key = (MemoryModuleType<object>) (object) memoryModuleType;
        ExpirableValue<object>? stored = _memories[key];
        return stored?.TimeToLive ?? 0L;
    }
    
    public bool IsMemoryValue<T>(MemoryModuleType<T> memoryModuleType, T value)
    {
        return HasMemoryValue(memoryModuleType) && GetMemory(memoryModuleType)!.Equals(value);
    }
    
    public bool CheckMemory<T>(MemoryModuleType<T> memoryModuleType, MemoryStatus memoryStatus)
    {
        var key = (MemoryModuleType<object>) (object) memoryModuleType;
        // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
        if (!_memories.ContainsKey(key)) return false;
        
        ExpirableValue<object>? memory = _memories[key];
        return memoryStatus switch
        {
            MemoryStatus.Registered => true,
            MemoryStatus.ValuePresent => memory.HasValue,
            MemoryStatus.ValueAbsent => !memory.HasValue,
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
    
    public Activity? GetActiveNonCoreActivity()
    {
        foreach (Activity activity in ActiveActivities)
        {
            if (!CoreActivities.Contains(activity))
            {
                return activity;
            }
        }

        return null;
    }
    
    public void SetActiveActivityIfPossible(Activity activity)
    {
        if (ActivityRequirementsAreMet(activity))
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
        foreach (var activeActivity in ActiveActivities.Where(activeActivity => !activeActivity.Equals(activity)))
        {
            if (!_activityMemoriesToEraseWhenStopped.TryGetValue(activeActivity, out var memoryTypes)) continue;
            
            foreach (MemoryModuleType<object> type in memoryTypes)
            {
                EraseMemory(type);
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
    
    public void SetActiveActivityToFirstValid(List<Activity> activities)
    {
        foreach (var activity in activities.Where(ActivityRequirementsAreMet))
        {
            SetActiveActivity(activity);
            break;
        }
    }
    
    public void AddActivity(Activity activity, int priority, IEnumerable<Behavior<TOwner>> behaviors)
    {
        AddActivity(activity, CreatePriorityTuples(priority, behaviors));
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
            var activity2Behs = _availableBehaviorsByPriority[tuple.Item1];
            if (activity2Behs == null)
            {
                activity2Behs = new Dictionary<Activity, HashSet<Behavior<TOwner>>>();
                _availableBehaviorsByPriority[tuple.Item1] = activity2Behs;
            }
            
            var behs = activity2Behs[act];
            if (behs == null)
            {
                behs = new HashSet<Behavior<TOwner>>();
                activity2Behs[act] = behs;
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
        foreach (KeyValuePair<MemoryModuleType<object>, ExpirableValue<object>?> entry in _memories)
        {
            MemoryModuleType<object> memoryModuleType = entry.Key;
            if (entry.Value.HasValue)
            {
                brain._memories[memoryModuleType] = entry.Value;
            }
        }
        return brain;
    }
    
    public void Update(IWorld world, TOwner owner)
    {
        ForgetOutdatedMemories();
        UpdateSensors(world, owner);
        StartEachNonRunningBehavior(world, owner);
        UpdateEachRunningBehavior(world, owner);
    }
    
    private void UpdateSensors(IWorld world, TOwner owner)
    {
        foreach (Sensor<ILivingEntity> sensor in _sensors.Values)
        {
            sensor.Update(world, owner);
        }
    }
    
    private void ForgetOutdatedMemories()
    {
        foreach (KeyValuePair<MemoryModuleType<object>, ExpirableValue<object>?> entry in _memories)
        {
            ExpirableValue<object>? expirableValue = entry.Value;
            if (!expirableValue.HasValue) continue;
            
            if (expirableValue.Value.IsExpired)
            {
                EraseMemory(entry.Key);
            }
            else
            {
                expirableValue.Value.Update();
            }
        }
    }
    
    public void StopAll(IWorld world, TOwner owner)
    {
        long gameTime = owner.World.GameTime;
        foreach (Behavior<TOwner> behavior in GetRunningBehaviors())
        {
            behavior.DoStop(world, owner, gameTime);
        }
    }
    
    private void StartEachNonRunningBehavior(IWorld world, TOwner owner)
    {
        long gameTime = world.GameTime;
        foreach (Dictionary<Activity, HashSet<Behavior<TOwner>>> actBehs in _availableBehaviorsByPriority.Values)
        {
            foreach (var (activity, behaviors) in actBehs)
            {
                if (ActiveActivities.Contains(activity)) continue;

                foreach (var behavior in behaviors.Where(behavior => behavior.Status == BehaviorStatus.Stopped))
                {
                    behavior.TryStart(world, owner, gameTime);
                }
            }
        }
    }
    
    private void UpdateEachRunningBehavior(IWorld world, TOwner owner)
    {
        long gameTime = world.GameTime;
        foreach (Behavior<TOwner> behavior in GetRunningBehaviors())
        {
            behavior.UpdateOrStop(world, owner, gameTime);
        }
    }
    
    private bool ActivityRequirementsAreMet(Activity activity)
    {
        if (!_activityRequirements.TryGetValue(activity, out var requirement)) return false;
        
        foreach (var (memoryModuleType, memoryStatus) in requirement)
        {
            if (!CheckMemory(memoryModuleType, memoryStatus))
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
        private readonly ExpirableValue<TU>? _value;

        public MemoryValue(MemoryModuleType<TU> type, ExpirableValue<TU>? value)
        {
            _type = type;
            _value = value;
        }

        public void SetMemoryInternal(Brain<TOwner> brain)
        {
            brain.SetMemoryInternal(_type, _value);
        }
    }
}