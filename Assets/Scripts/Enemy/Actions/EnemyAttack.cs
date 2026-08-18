using System;
using UnityEngine;

public abstract class EnemyAttack : MonoBehaviour, IEnemyAttack
{
    protected AttackAnimation attackAnimation;
    protected float range;
    protected float cooldownDuration;
    protected float priority;

    public event Action<IEntity> OnHit;
    public virtual float Range => range;
    public virtual float CooldownDuration => cooldownDuration;
    public virtual AttackAnimation AttackAnimation => attackAnimation;
    public virtual float Effectiveness => 1f;

    protected AttackHitbox AttackHitbox => attackHitbox;
    private AttackHitbox attackHitbox;
    

    public virtual void Initialize(EnemyAttackDefinition def)
    {
        range = def.range;
        cooldownDuration = def.cooldown;
        attackAnimation = def.animation;
        priority = def.priority;

        attackHitbox = GetComponentInChildren<AttackHitbox>();
        if (attackHitbox != null)
        {
            attackHitbox.OnHit += HandleHit;
        }
    }

    public float GetPriority() => priority;

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

    public virtual void OnAnimationFinish()
    {
    }
}