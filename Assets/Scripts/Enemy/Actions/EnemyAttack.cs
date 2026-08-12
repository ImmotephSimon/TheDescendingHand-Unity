using System;
using UnityEngine;

public abstract class EnemyAttack : MonoBehaviour, IEnemyAttack
{
    private AttackAnimation attackAnimation;
    private float range;
    private float cooldownDuration;

    public event Action<IEntity> OnHit;
    public float Range => range;
    public float CooldownDuration => cooldownDuration;
    public AttackAnimation AttackAnimation => attackAnimation;
    public float Effectiveness => 1f;

    protected AttackHitbox AttackHitbox => attackHitbox;
    private AttackHitbox attackHitbox;

    public virtual void Initialize(EnemyAttackDefinition def)
    {
        range = def.range;
        cooldownDuration = def.cooldown;
        attackAnimation = def.animation;

        attackHitbox = GetComponentInChildren<AttackHitbox>();
        if (attackHitbox != null)
        {
            attackHitbox.OnHit += HandleHit;
        }
    }

    private void HandleHit(IEntity entity) => OnHit?.Invoke(entity);

    public virtual bool CanHit(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        return direction.magnitude <= Range;
    }

    public virtual void Execute(Transform target)
    {
        attackHitbox?.Enable();
        Debug.Log($"Executing attack on {target.name}");
    }

    public virtual void Stop()
    {
        attackHitbox?.Disable();
    }
}