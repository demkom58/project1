namespace project1.addons.sbgoap.ai.behavior;

public interface IBehaviorControl
{
    public BehaviorStatus Status { get; }

    public bool TryStart(Brain brain, ulong gameTime);

    public void UpdateOrStop(Brain brain, ulong gameTime);

    public void DoStop(Brain brain, ulong gameTime);
}