using System.Collections.Generic;
using System.Linq;

namespace project1.scripts.world.entity.ai.schedule;

public class Schedule
{
    private readonly Dictionary<Activity, Timeline> _timelines = new();

    protected static Builder Create(string name)
    {
        return new Builder(name);
    }

    private Timeline EnsureTimelineExistsFor(Activity activity)
    {
        if (!_timelines.ContainsKey(activity)) _timelines.Add(activity, new Timeline());
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
            _name = name;
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
                schedule.EnsureTimelineExistsFor(activity);

            foreach (var activityTransition in _transitions)
            {
                var activity = activityTransition.Activity;
                foreach (var timeline in schedule.GetAllTimelinesExceptFor(activity))
                    timeline.AddKeyframe(activityTransition.Time, 0.0f);
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