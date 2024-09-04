using System.Collections.Generic;
using project1.scripts.world.entity.ai.memory;

namespace project1.scripts.world.entity.ai.sensor;

public abstract class Sensor<T> where T : ILivingEntity
{
    private readonly int _scanRate;
    private long _timeToTick;

    public Sensor(int scanRate = Sensors.DefaultScanRate)
    {
        _scanRate = scanRate;
        _timeToTick = Sensors.Random.Next(scanRate);
    }

    public void Update(IWorld world, T entity)
    {
        if (--_timeToTick > 0) return;
        _timeToTick = _scanRate;
        PostUpdate(world, entity);
    }

    protected abstract void PostUpdate(IWorld world, T entity);

    public abstract HashSet<MemoryModuleType<object>> Requires();

    public static bool IsEntityTargetable(ILivingEntity source, ILivingEntity target)
    {
        if (source.Brain.IsMemoryValue(MemoryModuleTypes.AttackTarget, target))
            return Sensors.TargetConditionsIgnoreInvisibilityTesting.Test(source, target);

        return Sensors.TargetingConditions.Test(source, target);
    }

    public static bool IsEntityAttackable(ILivingEntity source, ILivingEntity target)
    {
        if (source.Brain.IsMemoryValue(MemoryModuleTypes.AttackTarget, target))
            return Sensors.AttackTargetConditionsIgnoreInvisibilityTesting.Test(source, target);

        return Sensors.AttackTargetConditions.Test(source, target);
    }

    public static bool IsEntityAttackableIgnoringLineOfSight(ILivingEntity source, ILivingEntity target)
    {
        if (source.Brain.IsMemoryValue(MemoryModuleTypes.AttackTarget, target))
            return Sensors.AttackTargetConditionsIgnoreInvisibilityAndLineOfSight.Test(source, target);

        return Sensors.AttackTargetConditionsIgnoreLineOfSight.Test(source, target);
    }
}