using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using project1.addons.sbgoap.ai.behavior;
using project1.addons.sbgoap.ai.memory;

namespace project1.addons.sbgoap.ai.schedule;

[Tool]
public partial class Schedules : Node
{
    public static readonly Schedule Empty = new();
    
    private readonly Dictionary<Activity, HashSet<string>> _activityMemoriesToEraseWhenStopped = new();
    private readonly Dictionary<Activity, HashSet<Tuple<string, MemoryStatus>>> _activityRequirements = new();
    private readonly SortedDictionary<int, Dictionary<Activity, HashSet<Behavior>>> _availableBehaviorsByPriority = new();
    
    private HashSet<Activity> _coreActivities = new();
    public HashSet<Activity> CoreActivities
    {
        get => _coreActivities;
        set
        {
            CoreActivities.Clear();
            CoreActivities.UnionWith(value);
        }
    }
    public Schedule Schedule { get; set; } = Schedules.Empty;
    public HashSet<Activity> ActiveActivities { get; } = new();
    public Activity DefaultActivity { get; set; } = Activities.Idle;
    
    private long _lastScheduleUpdate;
    
    public override string[] _GetConfigurationWarnings()
    {
        if (GetParent() is not Brain) return new[] { "Node must be a child of a Brain node." };
        return base._GetConfigurationWarnings();
    }

    public override void _EnterTree()
    {
        if (GetParent() is not Brain brain) return;
        brain.Schedules = this;
    }

    public override void _ExitTree()
    {
        if (GetParent() is not Brain brain) return;
        brain.Schedules = null;
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
        var memories = GetParent() is Brain brain ? brain.Memories : null;
        if (memories == null)
        {
            GD.PushWarning("Memory node not found.");
            return;
        }
        
        foreach (var activeActivity in ActiveActivities.Where(activeActivity => !activeActivity.Equals(activity)))
        {
            if (!_activityMemoriesToEraseWhenStopped.TryGetValue(activeActivity, out var memoryTypes)) continue;

            foreach (var type in memoryTypes) memories.EraseMemory(type);
        }
    }

    public void UpdateActivityFromSchedule(long dayTime, long gameTime)
    {
        if (gameTime - _lastScheduleUpdate <= 20L) return;

        _lastScheduleUpdate = gameTime;
        var activity = Schedule.GetActivityAt((int)(dayTime % 24000L));
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
        string memoryType)
    {
        HashSet<Tuple<string, MemoryStatus>> memoriesRequirements
            = new() { new Tuple<string, MemoryStatus>(memoryType, MemoryStatus.ValuePresent) };

        HashSet<string> eraseOnStopMemories = new() { memoryType };

        AddActivityAndRemoveMemoriesWhenStopped(activity,
            CreatePriorityTuples(priority, behaviors), memoriesRequirements, eraseOnStopMemories);
    }

    public void AddActivity(Activity activity, IEnumerable<Tuple<int, Behavior>> behaviors)
    {
        AddActivityAndRemoveMemoriesWhenStopped(activity, behaviors,
            new HashSet<Tuple<string, MemoryStatus>>(), new HashSet<string>());
    }

    public void AddActivityWithConditions(
        Activity activity,
        IEnumerable<Tuple<int, Behavior>> behaviors,
        HashSet<Tuple<string, MemoryStatus>> set)
    {
        AddActivityAndRemoveMemoriesWhenStopped(activity, behaviors, set, new HashSet<string>());
    }

    public void AddActivityAndRemoveMemoriesWhenStopped(
        Activity act,
        IEnumerable<Tuple<int, Behavior>> behaviors,
        HashSet<Tuple<string, MemoryStatus>> memoriesRequirements,
        HashSet<string> eraseOnStopMemories)
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
    
    private bool ActivityRequirementsAreMet(Activity activity)
    {
        var memories = GetParent() is Brain brain ? brain.Memories : null;
        if (memories == null)
        {
            GD.PushWarning("Memory node not found.");
            return false;
        }
        
        if (!_activityRequirements.TryGetValue(activity, out var requirement)) return false;

        foreach (var (memoryModuleType, memoryStatus) in requirement)
            if (!memories.CheckMemory(memoryModuleType, memoryStatus))
                return false;
        
        return true;
    }

    private static List<Tuple<int, Behavior>> CreatePriorityTuples(
        int priority, 
        IEnumerable<Behavior> behaviors)
    {
        var priorityCounter = priority;
        return behaviors.Select(behavior => new Tuple<int, Behavior>(priorityCounter++, behavior)).ToList();
    }
}