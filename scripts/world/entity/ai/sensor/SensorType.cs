using System;

namespace project1.scripts.world.entity.ai.sensor;

public class SensorType<TSensor, TEntity> where TSensor : Sensor<TEntity> where TEntity : ILivingEntity
{
    public static readonly SensorType<DummySensor, TEntity> DUMMY = new SensorType<DummySensor, TEntity>("dummy", () => new DummySensor());
    public static readonly SensorType<NearestItemSensor, TEntity> NEAREST_ITEMS = new SensorType<NearestItemSensor, TEntity>("nearest_items", () => new NearestItemSensor());
    public static readonly SensorType<NearestLivingEntitySensor<TEntity>, TEntity> NEAREST_LIVING_ENTITIES = new SensorType<NearestLivingEntitySensor<TEntity>, TEntity>("nearest_living_entities", () => new NearestLivingEntitySensor<TEntity>());
    public static readonly SensorType<PlayerSensor, TEntity> NEAREST_PLAYERS = new SensorType<PlayerSensor, TEntity>("nearest_players", () => new PlayerSensor());
    private readonly Func<TSensor> _factory;

    public SensorType(Func<TSensor> factory)
    {
        this._factory = factory;
    }

    public TSensor Create()
    {
        return _factory();
    }
}