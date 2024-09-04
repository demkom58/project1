using System;
using System.Collections.Generic;
using Godot;
using project1.scripts.world.entity.ai.memory;

namespace project1.scripts.world.entity.ai.sensor;

/*

public abstract class Sensor<E extends LivingEntity> {
    private static final RandomSource RANDOM = RandomSource.createThreadSafe();
    private static final int DEFAULT_SCAN_RATE = 20;
    protected static final int TARGETING_RANGE = 16;
    private static final TargetingConditions TARGET_CONDITIONS = TargetingConditions.forNonCombat().range(16.0);
    private static final TargetingConditions TARGET_CONDITIONS_IGNORE_INVISIBILITY_TESTING = TargetingConditions.forNonCombat().range(16.0).ignoreInvisibilityTesting();
    private static final TargetingConditions ATTACK_TARGET_CONDITIONS = TargetingConditions.forCombat().range(16.0);
    private static final TargetingConditions ATTACK_TARGET_CONDITIONS_IGNORE_INVISIBILITY_TESTING = TargetingConditions.forCombat().range(16.0).ignoreInvisibilityTesting();
    private static final TargetingConditions ATTACK_TARGET_CONDITIONS_IGNORE_LINE_OF_SIGHT = TargetingConditions.forCombat().range(16.0).ignoreLineOfSight();
    private static final TargetingConditions ATTACK_TARGET_CONDITIONS_IGNORE_INVISIBILITY_AND_LINE_OF_SIGHT = TargetingConditions.forCombat().range(16.0).ignoreLineOfSight().ignoreInvisibilityTesting();
    private final int scanRate;
    private long timeToTick;

    public Sensor(int i) {
        this.scanRate = i;
        this.timeToTick = RANDOM.nextInt(i);
    }

    public Sensor() {
        this(20);
    }

    public final void tick(ServerLevel serverLevel, E livingEntity) {
        if (--this.timeToTick <= 0L) {
            this.timeToTick = this.scanRate;
            this.doTick(serverLevel, livingEntity);
        }
    }

    protected abstract void doTick(ServerLevel var1, E var2);

    public abstract Set<MemoryModuleType<?>> requires();

    public static boolean isEntityTargetable(LivingEntity livingEntity, LivingEntity livingEntity2) {
        if (livingEntity.getBrain().isMemoryValue(MemoryModuleType.ATTACK_TARGET, livingEntity2)) {
            return TARGET_CONDITIONS_IGNORE_INVISIBILITY_TESTING.test(livingEntity, livingEntity2);
        }
        return TARGET_CONDITIONS.test(livingEntity, livingEntity2);
    }

    public static boolean isEntityAttackable(LivingEntity livingEntity, LivingEntity livingEntity2) {
        if (livingEntity.getBrain().isMemoryValue(MemoryModuleType.ATTACK_TARGET, livingEntity2)) {
            return ATTACK_TARGET_CONDITIONS_IGNORE_INVISIBILITY_TESTING.test(livingEntity, livingEntity2);
        }
        return ATTACK_TARGET_CONDITIONS.test(livingEntity, livingEntity2);
    }

    public static boolean isEntityAttackableIgnoringLineOfSight(LivingEntity livingEntity, LivingEntity livingEntity2) {
        if (livingEntity.getBrain().isMemoryValue(MemoryModuleType.ATTACK_TARGET, livingEntity2)) {
            return ATTACK_TARGET_CONDITIONS_IGNORE_INVISIBILITY_AND_LINE_OF_SIGHT.test(livingEntity, livingEntity2);
        }
        return ATTACK_TARGET_CONDITIONS_IGNORE_LINE_OF_SIGHT.test(livingEntity, livingEntity2);
    }
}


*/
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
    
    public void Tick(Node level, T entity)
    {
        if (--_timeToTick > 0) return;
        _timeToTick = _scanRate;
        DoTick(level, entity);
    }

    protected abstract void DoTick(Node level, T entity);

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