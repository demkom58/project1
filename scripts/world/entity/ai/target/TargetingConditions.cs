using System;
using Godot;

namespace project1.scripts.world.entity.ai.target;

public readonly record struct TargetingConditions(
    bool IsCombat = false,
    double Range = -1.0,
    bool CheckLineOfSight = true,
    bool TestInvisible = true,
    Predicate<ILivingEntity>? Selector = null
)
{
    private const double MinVisibilityDistanceForInvisibleTarget = 2.0;

    public bool Test(ILivingEntity? src, ILivingEntity target)
    {
        if (src == target) return false;
        // if (!target.Interactable) return false;
        // if (Selector != null && !Selector(target)) return false;
        //
        // if (src == null)
        // {
        //     if (IsCombat && !target.Aggreble) return false;
        //
        //     return true;
        // }
        //
        // if (IsCombat && (!src.CanAttack(target) || !src.CanAttackType(target.Type) || src.IsAlliedTo(target)))
        //     return false;
        //
        // if (Range > 0.0)
        // {
        //     var visibilityPercent = TestInvisible ? target.GetVisibilityPercent(src) : 1.0;
        //     var maxRange = Mathf.Max(Range * visibilityPercent, MinVisibilityDistanceForInvisibleTarget);
        //     double distance = src.DistanceToSqr(target.Position);
        //     if (distance > maxRange * maxRange) return false;
        // }
        //
        // if (CheckLineOfSight && src is Mob mob && !mob.Sensing.HasLineOfSight(target)) return false;

        return true;
    }
}