using Godot;

namespace project1.addons.sbgoap.ai.memory;

public class MemoryModuleTypes
{
    public static readonly MemoryModuleType<Node3D> AttackTarget = new ("attack_target");

    private MemoryModuleTypes() { }
}