namespace project1.scripts.world.entity.ai.memory;

public class MemoryModuleTypes
{
    public static readonly MemoryModuleType<ILivingEntity> AttackTarget = new ("attack_target");

    private MemoryModuleTypes() { }
}