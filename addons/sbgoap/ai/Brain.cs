using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using project1.addons.sbgoap.ai.behavior;
using project1.addons.sbgoap.ai.memory;
using project1.addons.sbgoap.ai.schedule;
using project1.addons.sbgoap.ai.sensor;

namespace project1.addons.sbgoap.ai;

[Tool]
public partial class Brain : Node
{
    private readonly Dictionary<Activity, HashSet<MemoryModuleType<object>>>
        _activityMemoriesToEraseWhenStopped = new();

    private readonly Dictionary<Activity, HashSet<Tuple<MemoryModuleType<object>, MemoryStatus>>>
        _activityRequirements = new();

    private readonly SortedDictionary<int, Dictionary<Activity, HashSet<Behavior>>> _availableBehaviorsByPriority =
        new();

    private readonly Dictionary<MemoryModuleType<object>, ExpirableValue<object>?> _memories = new();
    private readonly HashSet<Sensor> _sensors = new();

    private long _lastScheduleUpdate;

    public Schedule Schedule { get; set; } = GOAPRegistry.ScheduleEmpty;
    public HashSet<Activity> CoreActivities { get; set; } = new();
    public HashSet<Activity> ActiveActivities { get; } = new();
    public Activity DefaultActivity { get; set; } = GOAPRegistry.ActivityIdle;

    public override void _Ready()
    {
        SetProcess(!Engine.IsEditorHint());
        SetPhysicsProcess(!Engine.IsEditorHint());
    }

    public bool HasMemoryValue<T>(MemoryModuleType<T> memoryModuleType)
    {
        return CheckMemory(memoryModuleType, MemoryStatus.ValuePresent);
    }

    public void ClearMemories()
    {
        foreach (var memoryModuleType in _memories.Keys) _memories[memoryModuleType] = new ExpirableValue<object>();
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
        _memories[key] = (ExpirableValue<object>?)(object?)value;
    }

    public T? GetMemory<T>(MemoryModuleType<T> memoryModuleType)
    {
        var key = (MemoryModuleType<object>)(object)memoryModuleType;

        if (!_memories.TryGetValue(key, out var stored))
            throw new InvalidOperationException("Unregistered memory fetched: " + memoryModuleType);


        return (T?)stored?.Value;
    }

    public T? GetMemoryInternal<T>(MemoryModuleType<T> memoryModuleType)
    {
        var key = (MemoryModuleType<object>)(object)memoryModuleType;
        var stored = _memories[key];
        return (T?)stored?.Value;
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

        if (!_memories.TryGetValue(key, out var memory)) return false;

        return memoryStatus switch
        {
            MemoryStatus.Registered => true,
            MemoryStatus.ValuePresent => memory != null,
            MemoryStatus.ValueAbsent => memory == null,
            _ => false
        };
    }

    public List<Behavior> GetRunningBehaviors()
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
        return ActiveActivities.FirstOrDefault(activity => !CoreActivities.Contains(activity));
    }

    public void SetActiveActivityIfPossible(Activity activity)
    {
        if (ActivityRequirementsAreMet(activity))
            SetActiveActivity(activity);
        else
            UseDefaultActivity();
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

            foreach (var type in memoryTypes) EraseMemory(type);
        }
    }

    public void UpdateActivityFromSchedule(long dayTime, long gameTime)
    {
        if (gameTime - _lastScheduleUpdate <= 20L) return;

        _lastScheduleUpdate = gameTime;
        var activity = Schedule.GetActivityAt(SBGOAP.DayTimeProvider(this));
        if (!ActiveActivities.Contains(activity)) SetActiveActivityIfPossible(activity);
    }

    public void SetActiveActivityToFirstValid(List<Activity> activities)
    {
        foreach (var activity in activities.Where(ActivityRequirementsAreMet))
        {
            SetActiveActivity(activity);
            break;
        }
    }

    public void AddActivity(Activity activity, int priority, IEnumerable<Behavior> behaviors)
    {
        AddActivity(activity, CreatePriorityTuples(priority, behaviors));
    }

    public void AddActivityAndRemoveMemoryWhenStopped(
        Activity activity,
        int priority,
        IEnumerable<Behavior> behaviors,
        MemoryModuleType<object> memoryType)
    {
        HashSet<Tuple<MemoryModuleType<object>, MemoryStatus>> memoriesRequirements
            = new() { new Tuple<MemoryModuleType<object>, MemoryStatus>(memoryType, MemoryStatus.ValuePresent) };

        HashSet<MemoryModuleType<object>> eraseOnStopMemories = new() { memoryType };

        AddActivityAndRemoveMemoriesWhenStopped(activity,
            CreatePriorityTuples(priority, behaviors), memoriesRequirements, eraseOnStopMemories);
    }

    public void AddActivity(Activity activity, IEnumerable<Tuple<int, Behavior>> behaviors)
    {
        AddActivityAndRemoveMemoriesWhenStopped(activity, behaviors,
            new HashSet<Tuple<MemoryModuleType<object>, MemoryStatus>>(), new HashSet<MemoryModuleType<object>>());
    }

    public void AddActivityWithConditions(
        Activity activity,
        IEnumerable<Tuple<int, Behavior>> behaviors,
        HashSet<Tuple<MemoryModuleType<object>, MemoryStatus>> set)
    {
        AddActivityAndRemoveMemoriesWhenStopped(activity, behaviors, set, new HashSet<MemoryModuleType<object>>());
    }

    public void AddActivityAndRemoveMemoriesWhenStopped(
        Activity act,
        IEnumerable<Tuple<int, Behavior>> behaviors,
        HashSet<Tuple<MemoryModuleType<object>, MemoryStatus>> memoriesRequirements,
        HashSet<MemoryModuleType<object>> eraseOnStopMemories)
    {
        _activityRequirements.Add(act, memoriesRequirements);
        if (eraseOnStopMemories.Count > 0) _activityMemoriesToEraseWhenStopped.Add(act, eraseOnStopMemories);
        foreach (var tuple in behaviors)
        {
            var activity2Behs = _availableBehaviorsByPriority[tuple.Item1];
            if (activity2Behs == null)
            {
                activity2Behs = new Dictionary<Activity, HashSet<Behavior>>();
                _availableBehaviorsByPriority[tuple.Item1] = activity2Behs;
            }

            var behs = activity2Behs[act];
            if (behs == null)
            {
                behs = new HashSet<Behavior>();
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

    public override void _PhysicsProcess(double delta)
    {
        ForgetOutdatedMemories();
        UpdateSensors();
        StartEachNonRunningBehavior();
        UpdateEachRunningBehavior();
    }

    private void UpdateSensors()
    {
        foreach (var sensor in _sensors) sensor.Update(this);
    }

    private void ForgetOutdatedMemories()
    {
        foreach (var (key, expirableValue) in _memories)
        {
            if (expirableValue == null) continue;

            if (expirableValue.IsExpired) EraseMemory(key);
            else expirableValue.Update();
        }
    }

    public void StopAll()
    {
        var gameTime = SBGOAP.GameTimeProvider(this);
        foreach (var behavior in GetRunningBehaviors()) behavior.DoStop(this, gameTime);
    }

    private void StartEachNonRunningBehavior()
    {
        var gameTime = SBGOAP.GameTimeProvider(this);
        foreach (var actBehs in _availableBehaviorsByPriority.Values)
        foreach (var (activity, behaviors) in actBehs)
        {
            if (ActiveActivities.Contains(activity)) continue;

            foreach (var behavior in behaviors.Where(behavior => behavior.Status == BehaviorStatus.Stopped))
                behavior.TryStart(this, gameTime);
        }
    }

    private void UpdateEachRunningBehavior()
    {
        var gameTime = SBGOAP.GameTimeProvider(this);
        foreach (var behavior in GetRunningBehaviors()) behavior.UpdateOrStop(this, gameTime);
    }

    private bool ActivityRequirementsAreMet(Activity activity)
    {
        if (!_activityRequirements.TryGetValue(activity, out var requirement)) return false;

        foreach (var (memoryModuleType, memoryStatus) in requirement)
            if (!CheckMemory(memoryModuleType, memoryStatus))
                return false;
        return true;
    }

    private static bool IsEmptyCollection(object obj)
    {
        return obj is ICollection { Count: 0 };
    }

    private static List<Tuple<int, Behavior>> CreatePriorityTuples(
        int priority,
        IEnumerable<Behavior> behaviors)
    {
        var priorityCounter = priority;
        return behaviors.Select(behavior => new Tuple<int, Behavior>(priorityCounter++, behavior)).ToList();
    }
}