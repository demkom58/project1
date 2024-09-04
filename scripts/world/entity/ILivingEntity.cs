using project1.scripts.world.entity.ai;

namespace project1.scripts.world.entity;

public interface ILivingEntity
{
    public Brain<ILivingEntity> Brain { get; }
}