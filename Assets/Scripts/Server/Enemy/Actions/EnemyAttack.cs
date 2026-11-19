using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyAttack : MonoBehaviour, IEnemyAttack
{
    protected AttackAnimation attackAnimation;
    protected float range;
    protected float cooldownDuration;
    protected float priority;
    private float swingDuration;

    public event Action<IEntity> OnHit;
    public virtual float Range => range;
    public virtual float CooldownDuration => cooldownDuration;
    public virtual AttackAnimation AttackAnimation => attackAnimation;
    public virtual float Effectiveness => 1f;

    public event Action Finished;

    protected virtual float SpeedMultiplier => 1f; // Need the slow/stat code
    protected virtual float SwingDuration => swingDuration;

    protected AttackHitbox AttackHitbox => attackHitbox;


    private AttackHitbox attackHitbox;
    private Coroutine _swing;

    private bool _hasHitbox;

    public virtual void Initialize(EnemyAttackDefinition def)
    {
        range = def.range;
        cooldownDuration = def.cooldown;
        attackAnimation = def.animation;
        priority = def.priority;
        swingDuration = def.swingDuration;
    }

    public virtual void Initialize(EnemyAttackDefinition def, List<HitBoxSample> hitboxSamples, Transform visualRoot)
    {
        Initialize(def);

        attackHitbox = new AttackHitbox();
        attackHitbox.Setup(hitboxSamples, visualRoot, def.hitWindow);
        _hasHitbox = true;
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
        if (!_hasHitbox) return;
        attackHitbox.OnHit -= HandleHit;
        attackHitbox.OnHit += HandleHit;
        _swing = StartCoroutine(Swing());
    }

    private IEnumerator Swing()
    {
        attackHitbox.Enable();

        float progress = 0f;
        while (progress < 1f)
        {
            attackHitbox.UpdateHitbox(progress);
            progress += Time.deltaTime * SpeedMultiplier / SwingDuration;
            yield return null;
        }

        attackHitbox.Disable();
        _swing = null;
        Finished?.Invoke();
    }

    public virtual void Stop()
    {
        if (_swing != null) StopCoroutine(_swing);
        _swing = null;
        //if (!AttackHitbox) return;
        AttackHitbox.Disable();
        AttackHitbox.OnHit -= HandleHit;
    }

    public virtual void OnAnimationFinish()
    {
    }
}