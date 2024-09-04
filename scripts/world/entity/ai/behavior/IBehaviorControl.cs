namespace project1.scripts.world.entity.ai.behavior;

public interface IBehaviorControl<T> where T : ILivingEntity
{
    public BehaviorStatus Status { get; }

    public bool TryStart(IWorld level, T entity, long gameTime);

    public void UpdateOrStop(IWorld level, T entity, long gameTime);

    public void DoStop(IWorld level, T entity, long gameTime);
}