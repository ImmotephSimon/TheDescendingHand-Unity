using System;
using System.Collections.Generic;
using UnityEngine;

public interface IEntity
{
    bool IsDead { get;  }
    Transform Transform { get; }

    event Action<IEntity> Died;

    IStatContainer Stats { get; }

    /// <summary>
    /// Target team layer for physics/queries
    /// </summary>
    abstract int HostileLayer { get; }

    /// <summary>
    /// Layer assigned to spawned damage sources
    /// </summary>
    abstract int AttackLayer { get; }

    Vector3 CursorPosition { get; }
    List<(StatModifier, float)> OnHitStats { get; }

    void Die(IEntity killer);
}