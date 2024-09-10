using System;

namespace project1.addons.sbgoap.ai.behavior;

public interface IBehaviorControl
{
    public BehaviorStatus Status { get; }

    public bool TryStart(long gameTime);

    public void UpdateOrStop(long gameTime);

    public void DoStop(long gameTime);
}