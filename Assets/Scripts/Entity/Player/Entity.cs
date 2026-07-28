using System;
using System.Collections;
using UnityEngine;

public abstract class Entity : MonoBehaviour, IEntity, IDamageable, IStunnable
{
    protected IAnimationHandler animationHandler;
    protected IStatContainer stats;
    private MitigationLayer mitigationLayer;
    private Coroutine _stunRoutine;

    public bool IsDead { get; protected set; }

    public Transform Transform => transform;

    public IStatContainer Stats => stats;

    public int TeamLayer => gameObject.layer;

    protected virtual void Awake()
    {
        mitigationLayer = GetComponent<MitigationLayer>();
        Debug.Assert(mitigationLayer != null, $"{name} missing MitigationLayer");
    }
    protected virtual void OnEnable()
    {
        GameWorld.Instance.RegisterEntity(this);
        GameWorld.Instance.EntityDied += OnEntityDied;
        GameWorld.Instance.EntityRevived += OnEntityRevived;
    }

    protected virtual void OnEntityRevived(IEntity entity){}

    protected abstract void OnEntityDied(IEntity victim, IEntity killer);

    protected virtual void OnDisable()
    {
        if (GameWorld.Instance == null) return;

        GameWorld.Instance.UnregisterEntity(this);
        GameWorld.Instance.EntityDied -= OnEntityDied;
        GameWorld.Instance.EntityRevived -= OnEntityRevived;
    }

    public void Die(IEntity killer)
    {
        if (IsDead) return;

        IsDead = true;
        StopCoroutine(_stunRoutine);
        OnDeath(killer);
        animationHandler.SetAnimationState(CharacterAnimationState.Dead);
        GameWorld.Instance.NotifyDeath(this, killer);
    }

    public void TakeDamage(DamageInfo info)
    {
        mitigationLayer.TakeDamage(info);
    }

    public virtual void ApplyStun(float duration)
    {
        OnStunBegin();
        animationHandler.SetAnimationState(CharacterAnimationState.Stun);

        _stunRoutine =  StartCoroutine(StunRoutine(duration));
    }

    protected IEnumerator StunRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        animationHandler.SetAnimationState(CharacterAnimationState.Locomotion);
        OnStunEnd();
    }

    protected virtual void OnDeath(IEntity killer) { }
    protected virtual void OnStunBegin(){}
    protected virtual void OnStunEnd(){}
}