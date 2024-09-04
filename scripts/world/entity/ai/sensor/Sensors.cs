using System;
using project1.scripts.world.entity.ai.target;

namespace project1.scripts.world.entity.ai.sensor;

public class Sensors
{
    public const int DefaultScanRate = 20;
    public const int TargetingRange = 16;

    public static readonly Random Random = new();

    public static readonly TargetingConditions TargetingConditions
        = new() { IsCombat = false, Range = TargetingRange };
    public static readonly TargetingConditions TargetConditionsIgnoreInvisibilityTesting
        = new() { IsCombat = false, Range = TargetingRange, TestInvisible = false };
    public static readonly TargetingConditions AttackTargetConditions =
        new() { IsCombat = true, Range = TargetingRange };
    public static readonly TargetingConditions AttackTargetConditionsIgnoreInvisibilityTesting =
        new() { IsCombat = true, Range = TargetingRange, TestInvisible = false };
    public static readonly TargetingConditions AttackTargetConditionsIgnoreLineOfSight =
        new() { IsCombat = true, Range = TargetingRange, CheckLineOfSight = false };
    public static readonly TargetingConditions AttackTargetConditionsIgnoreInvisibilityAndLineOfSight =
        new() { IsCombat = true, Range = TargetingRange, TestInvisible = false, CheckLineOfSight = false };
    
    public static readonly SensorType<DummySensor, ILivingEntity> Dummy =
        new SensorType<DummySensor, ILivingEntity>("dummy", () => new DummySensor());
    public static readonly SensorType<NearestItemSensor, ILivingEntity> NearestItems =
        new SensorType<NearestItemSensor, ILivingEntity>("nearest_items", () => new NearestItemSensor());
    public static readonly SensorType<NearestLivingEntitySensor<ILivingEntity>, ILivingEntity> NearestLivingEntities =
        new SensorType<NearestLivingEntitySensor<ILivingEntity>, ILivingEntity>("nearest_living_entities",
            () => new NearestLivingEntitySensor<ILivingEntity>());
    public static readonly SensorType<PlayerSensor, ILivingEntity> NearestPlayers =
        new SensorType<PlayerSensor, ILivingEntity>("nearest_players", () => new PlayerSensor());
}