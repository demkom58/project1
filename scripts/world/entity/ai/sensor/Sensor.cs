using System;
using System.Collections.Generic;
using Godot;
using project1.scripts.world.entity.ai.memory;

namespace project1.scripts.world.entity.ai.sensor;

public abstract class Sensor<T> where T : ILivingEntity
{
    private const int DefaultScanRate = 20;
    private const int TargetingRange = 16;
    
    private static readonly Random Random = new();
    private static readonly TargetingConditions TARGET_CONDITIONS = TargetingConditions.ForNonCombat().Range(TargetingRange);

    private static readonly TargetingConditions TARGET_CONDITIONS_IGNORE_INVISIBILITY_TESTING =
        TargetingConditions.ForNonCombat().Range(16.0).IgnoreInvisibilityTesting();

    private static readonly TargetingConditions ATTACK_TARGET_CONDITIONS = TargetingConditions.ForCombat().Range(TargetingRange);

    private static readonly TargetingConditions ATTACK_TARGET_CONDITIONS_IGNORE_INVISIBILITY_TESTING =
        TargetingConditions.ForCombat().Range(16.0).IgnoreInvisibilityTesting();

    private static readonly TargetingConditions ATTACK_TARGET_CONDITIONS_IGNORE_LINE_OF_SIGHT =
        TargetingConditions.ForCombat().Range(16.0).IgnoreLineOfSight();

    private static readonly TargetingConditions ATTACK_TARGET_CONDITIONS_IGNORE_INVISIBILITY_AND_LINE_OF_SIGHT =
        TargetingConditions.ForCombat().Range(16.0).IgnoreLineOfSight().IgnoreInvisibilityTesting();

    private readonly int _scanRate;
    private long _timeToTick;

    public Sensor(int scanRate = DefaultScanRate)
    {
        _scanRate = scanRate;
        _timeToTick = Random.Next(scanRate);
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
        if (source.Brain.IsMemoryValue(MemoryModuleType<object>.ATTACK_TARGET, target))
        {
            return TARGET_CONDITIONS_IGNORE_INVISIBILITY_TESTING.Test(source, target);
        }

        return TARGET_CONDITIONS.Test(source, target);
    }

    public static bool IsEntityAttackable(ILivingEntity source, ILivingEntity target)
    {
        if (source.Brain.IsMemoryValue(MemoryModuleType<object>.ATTACK_TARGET, target))
        {
            return ATTACK_TARGET_CONDITIONS_IGNORE_INVISIBILITY_TESTING.Test(source, target);
        }

        return ATTACK_TARGET_CONDITIONS.Test(source, target);
    }

    public static bool IsEntityAttackableIgnoringLineOfSight(ILivingEntity source, ILivingEntity target)
    {
        if (source.Brain.IsMemoryValue(MemoryModuleType<object>.ATTACK_TARGET, target))
        {
            return ATTACK_TARGET_CONDITIONS_IGNORE_INVISIBILITY_AND_LINE_OF_SIGHT.Test(source, target);
        }

        return ATTACK_TARGET_CONDITIONS_IGNORE_LINE_OF_SIGHT.Test(source, target);
    }
}