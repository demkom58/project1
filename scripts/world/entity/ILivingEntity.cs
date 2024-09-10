using project1.addons.sbgoap.ai;

namespace project1.scripts.world.entity;

public interface ILivingEntity
{
    public Brain Brain { get; }
    
    public IWorld World { get; }
}