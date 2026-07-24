using System;
using UnityEngine;

public abstract class EnemyAttack : MonoBehaviour, IEnemyAttack
{
    [SerializeField] private AttackAnimation attackAnimation;
    [SerializeField] private float range = 2f;
    [SerializeField] private float attackAngle = 45f;
    [SerializeField] private AttackHitbox attackHitbox;
    protected AttackHitbox AttackHitbox => attackHitbox;

    public event Action<IEntity> OnHit;

    public virtual float Range => range;

    public virtual float CooldownDuration => 2f;

    public virtual AttackAnimation AttackAnimation => attackAnimation;

    private void HandleHit(IEntity entity)
    {
        OnHit?.Invoke(entity);
    }

    private void Awake()
    {
        attackHitbox.OnHit += HandleHit;
    }

    public virtual bool CanHit(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.magnitude > range)
            return false;

        float angle = Vector3.Angle(transform.forward, direction);

        return angle <= attackAngle;
    }

    public virtual void Execute(Transform target)
    {
        attackHitbox.Enable();
        Debug.Log($"Executing attack on {target.name}");
    }
}