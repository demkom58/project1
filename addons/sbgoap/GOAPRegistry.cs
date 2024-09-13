using System;
using project1.addons.sbgoap.ai.behavior;
using project1.addons.sbgoap.ai.schedule;
using project1.addons.sbgoap.ai.sensor;

namespace project1.addons.sbgoap;

using System.Collections.Generic;

public static class GOAPRegistry
{
    public static Schedule ScheduleEmpty {get;} = new Schedule();
    public static Activity ActivityIdle {get;} = new Activity("Idle");
    
    private static readonly Dictionary<string, Type> _memories  = new();
    private static readonly Dictionary<string, Schedule> _schedules = new();
    private static readonly Dictionary<string, Activity> _activities = new();
    private static readonly Dictionary<string, Type> _behaviors = new();
    private static readonly Dictionary<string, Type> _sensors = new();
    
    public static IReadOnlyDictionary<string, Type> Memories => _memories.AsReadOnly();
    public static IReadOnlyDictionary<string, Schedule> Schedules => _schedules.AsReadOnly();
    public static IReadOnlyDictionary<string, Activity> Activities => _activities.AsReadOnly();
    public static IReadOnlyDictionary<string, Type> Behaviors => _behaviors.AsReadOnly();
    public static IReadOnlyDictionary<string, Type> Sensors => _sensors.AsReadOnly();

    public static bool RegisterMemory<T>(string memoryName)
    {
        return _memories.TryAdd(memoryName, typeof(T));
    }

    public static bool RegisterSchedule(string scheduleName, Schedule schedule)
    {
        return _schedules.TryAdd(scheduleName, schedule);
    }

    public static bool RegisterActivity(string activityName)
    {
        return _activities.TryAdd(activityName, new Activity(activityName));
    }

    public static bool RegisterBehavior<T>(string behaviorName) where T : IBehaviorControl
    {
        var type = typeof(T);
        if (type.IsAbstract || type.IsInterface) return false;
        return _behaviors.TryAdd(behaviorName, type);
    }

    public static bool RegisterSensor<T>(string sensorName) where T : Sensor
    {
        var type = typeof(T);
        if (type.IsAbstract || type.IsInterface) return false;
        return _sensors.TryAdd(sensorName, type);
    }

    public static void Init()
    {
        _schedules["Empty"] = ScheduleEmpty;
        _activities["Idle"] = ActivityIdle;
    }
}