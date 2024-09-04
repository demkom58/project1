using System.Collections.Generic;
using System.Linq;

namespace project1.scripts.world.entity.ai.schedule;

/*
public class ScheduleBuilder {
    private final Schedule schedule;
    private final List<ActivityTransition> transitions = Lists.newArrayList();

    public ScheduleBuilder(Schedule schedule) {
        this.schedule = schedule;
    }

    public ScheduleBuilder changeActivityAt(int i, Activity activity) {
        this.transitions.add(new ActivityTransition(i, activity));
        return this;
    }

    public Schedule build() {
        this.transitions.stream().map(ActivityTransition::getActivity).collect(Collectors.toSet()).forEach(this.schedule::ensureTimelineExistsFor);
        this.transitions.forEach(activityTransition -> {
            Activity activity = activityTransition.getActivity();
            this.schedule.getAllTimelinesExceptFor(activity).forEach(timeline -> timeline.addKeyframe(activityTransition.getTime(), 0.0f));
            this.schedule.getTimelineFor(activity).addKeyframe(activityTransition.getTime(), 1.0f);
        });
        return this.schedule;
    }

    static class ActivityTransition {
        private final int time;
        private final Activity activity;

        public ActivityTransition(int i, Activity activity) {
            this.time = i;
            this.activity = activity;
        }

        public int getTime() {
            return this.time;
        }

        public Activity getActivity() {
            return this.activity;
        }
    }
}


*/
