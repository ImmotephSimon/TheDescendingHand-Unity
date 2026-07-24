using System;
using UnityEngine;

public interface IEnemyAttack
{
    event Action<IEntity> OnHit;
    float Range { get; }
    float CooldownDuration { get; }
    AttackAnimation AttackAnimation { get; }

    bool CanHit(Transform target);
    void Execute(Transform target);
}