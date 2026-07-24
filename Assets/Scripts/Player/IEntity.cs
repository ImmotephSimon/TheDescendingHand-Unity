using UnityEngine;

public interface IEntity
{
    bool IsDead { get;  }
    Transform Transform { get; }
    int TeamLayer { get; }

    IStatContainer Stats { get; }

    public int TargetTeamLayer => TeamLayer == LayerMask.NameToLayer("Player")
        ? LayerMask.NameToLayer("Enemy")
        : LayerMask.NameToLayer("Player");

    void Die();
}