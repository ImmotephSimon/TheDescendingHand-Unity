using UnityEngine;

public interface IAttackAbility
{
    float Range { get; }
    float CooldownDuration { get; }
    AnimationClip AttackAnimation { get; }

    bool CanHit(Transform target);
    void Execute(Transform target);
}