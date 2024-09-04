using Godot;

namespace project1.scripts.world.entity.ai.behavior;

public interface IBehaviorControl<T> where T : ILivingEntity
{
    public BehaviorStatus Status { get; }
    
    public bool TryStart(Node level, T entity, long updateNumber);
    
    public void TickOrStop(Node level, T entity, long updateNumber);
    
    public void DoStop(Node level, T entity, long updateNumber);
}