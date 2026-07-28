using System;
using UnityEngine;

public interface IEntity
{
    bool IsDead { get;  }
    Transform Transform { get; }
    int TeamLayer { get; }

    IStatContainer Stats { get; }

    public int OtherTeamLayer => TeamLayer == LayerMask.NameToLayer("Player")
        ? LayerMask.NameToLayer("Enemy")
        : LayerMask.NameToLayer("Player");

    void Die(IEntity killer);
}