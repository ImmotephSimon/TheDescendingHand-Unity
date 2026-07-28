using System;
using UnityEngine;

public abstract class EnemyAttack : MonoBehaviour, IEnemyAttack
{
    [SerializeField] private AttackAnimation attackAnimation;
    
    public event Action<IEntity> OnHit;
    public virtual float Range => 1.2f;
    public virtual float CooldownDuration => 0f;
    public virtual AttackAnimation AttackAnimation => attackAnimation;
    public float Effectiveness => 1f;
    protected AttackHitbox AttackHitbox => attackHitbox;
    private AttackHitbox attackHitbox;

    private void Awake()
    {
        attackHitbox = GetComponentInChildren<AttackHitbox>();
        attackHitbox.OnHit += HandleHit;
    }

    private void HandleHit(IEntity entity)
    {
        OnHit?.Invoke(entity);
    }

    public virtual bool CanHit(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        return direction.magnitude <= Range;
    }

    public virtual void Execute(Transform target)
    {
        attackHitbox.Enable();
        Debug.Log($"Executing attack on {target.name}");
    }

    public virtual void Stop()
    {
        attackHitbox.Disable();
    }
}