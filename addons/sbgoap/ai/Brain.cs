using System;
using Godot;
using project1.addons.sbgoap.ai.behavior;
using project1.addons.sbgoap.ai.memory;
using project1.addons.sbgoap.ai.schedule;
using project1.addons.sbgoap.ai.sensor;

namespace project1.addons.sbgoap.ai;

[Tool]
public partial class Brain : Node
{
    public Memories? Memories { get; set; }
    public Sensors? Sensors { get; set; }
    public Behaviors? Behaviors { get; set; }
    public Schedules? Schedules { get; set; }

    public Brain()
    {
        ChildEnteredTree += node =>
        {
            switch (node)
            {
                case Memories newMemories when Memories != null:
                    throw new InvalidOperationException("Brain already has a Memories node.");
                case Memories newMemories:
                    Memories = newMemories;
                    break;
                
                case Sensors newSensor when Sensors != null:
                    throw new InvalidOperationException("Brain already has a Sensors node.");
                case Sensors newSensor:
                    Sensors = newSensor;
                    break;
                
                case Behaviors newBehaviors when Behaviors != null:
                    throw new InvalidOperationException("Brain already has a Behaviors node.");
                case Behaviors newBehaviors:
                    Behaviors = newBehaviors;
                    break;
                
                case Schedules newSchedules when Schedules != null:
                    throw new InvalidOperationException("Brain already has a Schedules node.");
                case Schedules newSchedules:
                    Schedules = newSchedules;
                    break;
            }
        };
        
        ChildExitingTree += node =>
        {
            switch (node)
            {
                case Memories oldMemories when Memories == oldMemories:
                    Memories = null;
                    break;
                
                case Sensors oldSensor when Sensors == oldSensor:
                    Sensors = null;
                    break;
                
                case Behaviors oldBehaviors when Behaviors == oldBehaviors:
                    Behaviors = null;
                    break;
                
                case Schedules oldSchedules when Schedules == oldSchedules:
                    Schedules = null;
                    break;
            }
        };
        
        // foreach (var type in memoryTypes) _memories.Add(type, new ExpirableValue<object>());
        //
        // foreach (var sensorType in sensors) _sensors.AddRange(sensorType, sensorType.Create());
        //
        //
        // foreach (var sensor in _sensors.Values)
        // foreach (var required in sensor.Requires())
        //     _memories.Add(required, new ExpirableValue<object>());
        //
        // foreach (var memoryValue in memories) memoryValue.SetMemoryInternal(this);
    }
    
    public Brain CopyWithoutBehaviors()
    {
        // var brain = new Brain(_memories.Keys, _sensors.Keys, ImmutableList<MemoryValue<object>>.Empty);
        // foreach (var (memoryModuleType, value) in _memories)
        // {
        //     if (value.HasValue) brain._memories[memoryModuleType] = value;
        // }
        //
        // return brain;
        return null;
    }

    public void Update()
    {
        // ForgetOutdatedMemories();
        // UpdateSensors();
        // StartEachNonRunningBehavior();
        // UpdateEachRunningBehavior();
    }

    private void UpdateSensors()
    {
        // foreach (var sensor in _sensors.Values) sensor.Update();
    }

    public void StopAll()
    {
        // var gameTime = owner.World.GameTime;
        // foreach (var behavior in GetRunningBehaviors()) behavior.DoStop(gameTime);
    }

    private void StartEachNonRunningBehavior()
    {
        // var gameTime = world.GameTime;
        // foreach (var actBehs in _availableBehaviorsByPriority.Values)
        // foreach (var (activity, behaviors) in actBehs)
        // {
        //     if (ActiveActivities.Contains(activity)) continue;
        //
        //     foreach (var behavior in behaviors.Where(behavior => behavior.Status == BehaviorStatus.Stopped))
        //         behavior.TryStart(gameTime);
        // }
    }

    private void UpdateEachRunningBehavior()
    {
        // var gameTime = world.GameTime;
        // foreach (var behavior in GetRunningBehaviors()) behavior.UpdateOrStop(gameTime);
    }

}