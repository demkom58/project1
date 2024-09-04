using System.Collections.Generic;
using System.Linq;

namespace project1.scripts.world.entity.ai.schedule;

/*
public class Schedule {
    public static final int WORK_START_TIME = 2000;
    public static final int TOTAL_WORK_TIME = 7000;
    public static final Schedule EMPTY = Schedule.register("empty").changeActivityAt(0, Activity.IDLE).build();
    public static final Schedule SIMPLE = Schedule.register("simple").changeActivityAt(5000, Activity.WORK).changeActivityAt(11000, Activity.REST).build();
    public static final Schedule VILLAGER_BABY = Schedule.register("villager_baby").changeActivityAt(10, Activity.IDLE).changeActivityAt(3000, Activity.PLAY).changeActivityAt(6000, Activity.IDLE).changeActivityAt(10000, Activity.PLAY).changeActivityAt(12000, Activity.REST).build();
    public static final Schedule VILLAGER_DEFAULT = Schedule.register("villager_default").changeActivityAt(10, Activity.IDLE).changeActivityAt(2000, Activity.WORK).changeActivityAt(9000, Activity.MEET).changeActivityAt(11000, Activity.IDLE).changeActivityAt(12000, Activity.REST).build();
    private final Map<Activity, Timeline> timelines = Maps.newHashMap();

    protected static ScheduleBuilder register(String string) {
        Schedule schedule = Registry.register(BuiltInRegistries.SCHEDULE, string, new Schedule());
        return new ScheduleBuilder(schedule);
    }

    protected void ensureTimelineExistsFor(Activity activity) {
        if (!this.timelines.containsKey(activity)) {
            this.timelines.put(activity, new Timeline());
        }
    }

    protected Timeline getTimelineFor(Activity activity) {
        return this.timelines.get(activity);
    }

    protected List<Timeline> getAllTimelinesExceptFor(Activity activity) {
        return this.timelines.entrySet().stream().filter(entry -> entry.getKey() != activity).map(Map.Entry::getValue).collect(Collectors.toList());
    }

    public Activity getActivityAt(int i) {
        return this.timelines.entrySet().stream().max(Comparator.comparingDouble(entry -> ((Timeline)entry.getValue()).getValueAt(i))).map(Map.Entry::getKey).orElse(Activity.IDLE);
    }
}


*/
public class Schedule
{
    public static readonly int WorkStartTime = 2000;
    public static readonly int TotalWorkTime = 7000;
    public static readonly Schedule Empty = new ();
    public static readonly Schedule Simple = new ();
    public static readonly Schedule VillagerBaby = new ();
    public static readonly Schedule VillagerDefault = new ();
    
    private readonly Dictionary<Activity, Timeline> _timelines = new ();
    
    protected static Builder Create(string name) {
        return new Builder(name);
    }
    
    private Timeline EnsureTimelineExistsFor(Activity activity)
    {
        if (!_timelines.ContainsKey(activity))
        {
            _timelines.Add(activity, new Timeline());
        }
        return _timelines[activity];
    }
    
    private Timeline GetTimelineFor(Activity activity)
    {
        return _timelines[activity];
    }
    
    private List<Timeline> GetAllTimelinesExceptFor(Activity activity)
    {
        return _timelines.Where(entry
            => entry.Key.Equals(activity)).Select(entry => entry.Value).ToList();
    }
    
    public Activity GetActivityAt(int timeStamp)
    {
        return _timelines.Aggregate((l, r)
            => l.Value.GetValueAt(timeStamp) > r.Value.GetValueAt(timeStamp) ? l : r).Key;
    }
    
    public class Builder
    {
        private readonly string _name;
        private readonly List<ActivityTransition> _transitions = new();
    
        public Builder(string name)
        {
            this._name = name;
        }

        public Builder ChangeActivityAt(int i, Activity activity)
        {
            _transitions.Add(new ActivityTransition(i, activity));
            return this;
        }

        public Schedule Register()
        {
            var schedule = new Schedule();
            foreach (var activity in _transitions.Select(t => t.Activity).ToHashSet())
            {
                schedule.EnsureTimelineExistsFor(activity);
            }
        
            foreach (var activityTransition in _transitions)
            {
                var activity = activityTransition.Activity;
                foreach (var timeline in schedule.GetAllTimelinesExceptFor(activity))
                {
                    timeline.AddKeyframe(activityTransition.Time, 0.0f);
                }
                schedule.GetTimelineFor(activity).AddKeyframe(activityTransition.Time, 1.0f);
            }
        
            // Registry.register(BuiltInRegistries.SCHEDULE, _name, schedule);
            
            return schedule;
        }

        private struct ActivityTransition
        {
            public readonly int Time;
            public readonly Activity Activity;

            public ActivityTransition(int time, Activity activity)
            {
                Time = time;
                Activity = activity;
            }
        }
    }
}