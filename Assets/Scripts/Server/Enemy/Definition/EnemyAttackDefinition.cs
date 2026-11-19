using UnityEngine;

public abstract class EnemyAttackDefinition : ScriptableObject
{
    public float range = 1.5f;
    public float cooldown = 1f;
    public AttackAnimation animation;
    public float priority = 100f;

    [HideInInspector] public AttackHitbox.HitWindow hitWindow; // baked
    [HideInInspector] public float swingDuration;              // baked

    public abstract IEnemyAttack Create(ServerEnemy owner);
}