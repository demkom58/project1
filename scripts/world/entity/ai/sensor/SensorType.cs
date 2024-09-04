using System;

namespace project1.scripts.world.entity.ai.sensor;

public class SensorType<TSensor, TEntity> where TSensor : Sensor<TEntity> where TEntity : ILivingEntity
{
    private readonly Func<TSensor> _factory;

    public SensorType(Func<TSensor> factory)
    {
        _factory = factory;
    }

    public TSensor Create()
    {
        return _factory();
    }
}