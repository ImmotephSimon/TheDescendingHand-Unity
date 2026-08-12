using UnityEngine;

public abstract class EnemyAttackDefinition : ScriptableObject
{
    public float range = 1.5f;
    public float cooldown = 1f;
    public AttackAnimation animation;

    public abstract IEnemyAttack Create(Enemy owner);
}