using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour, IEntity, IDamageable, IStunnable
{
    [SerializeField] private TagReactions tagReactions;

    protected IAnimationHandler _animationHandler;
    protected IStatContainer _stats;
    protected DegenComponent _degen;
    private Coroutine _stunRoutine;
    private IHealth _healthHandler;
    private IAilmentHandler _ailmentHandler;
    private ModifierHandle _stunStatHandle;
    private DamagePipelineComponent _damagePipeline;

    public int HostileLayer => TeamLayer == LayerMask.NameToLayer("Player")
    ? LayerMask.NameToLayer("Enemy")
    : LayerMask.NameToLayer("Player");
    public int AttackLayer => TeamLayer == LayerMask.NameToLayer("Player")
    ? LayerMask.NameToLayer("PlayerAttack")
    : LayerMask.NameToLayer("EnemyAttack");

    public bool IsDead { get; protected set; }

    public Transform Transform => transform;

    public IStatContainer Stats => _stats;

    protected int TeamLayer => gameObject.layer;

    public virtual Vector3 CursorPosition { get; protected set; }

    public List<(StatModifier, float)> OnHitStats { get; } = new();

    public event Action<IEntity> Died;

    protected virtual void Awake()
    {
        _stats = GetComponent<IStatContainer>();
        _healthHandler = GetComponent<IHealth>();
        _ailmentHandler = GetComponent<IAilmentHandler>();
        Debug.Assert(_stats != null, $"{name} missing stats");
        Debug.Assert(_ailmentHandler != null, $"{name} missing ailment handler");
        Debug.Assert(tagReactions != null, $"{name} missing TagReactions");

        _degen = gameObject.AddComponent<DegenComponent>();
        Debug.Assert(GetComponents<DegenComponent>().Length == 1, $"{name} has problems regarding DegenComponent");
        _damagePipeline = gameObject.AddComponent<DamagePipelineComponent>();
        Debug.Assert(GetComponents<DamagePipelineComponent>().Length == 1, $"{name} has problems regarding DamagePipeline");

        foreach (var reaction in tagReactions.Reactions)
            reaction.StartListening(this);
    }
    protected virtual void Start()
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
        if (killer == null)
            Debug.LogWarning($"Die {name} with NULL killer");

        IsDead = true;
        if (_stunRoutine != null)
        {
            Debug.Log($"CANCEL STUN {name}");
            StopCoroutine(_stunRoutine);
            _stats.RemoveModifier(_stunStatHandle);
            _stunRoutine = null;
        }
        OnDeath(killer);
        Died?.Invoke(this);
        _animationHandler?.SetAnimationState(CharacterAnimationState.Dead);
        GameWorld.Instance.NotifyDeath(this, killer);
    }

    public virtual void TakeDamage(DamageInfo info)
    {
        foreach ((StatModifier stat, float duration) in info.Source.OnHitStats)
        {
            _stats.AddModifier(stat, duration);
        }

        if (!_damagePipeline.RunPreProcess(this, info, out float finalDamage))
        {
            Debug.Log($"[Entity:TakeDamage] Something logical prevented damage application.");
            return;
        }

        _healthHandler.AdjustHealth(-finalDamage, info.Source);

        _ailmentHandler.ApplyAilments(info);
        _damagePipeline.RunPostProcess(this, info, finalDamage);
    }

    public virtual void ApplyStun(float duration)
    {
        if (IsDead) return;
        if (_stunRoutine != null)
        {
            StopCoroutine(_stunRoutine);
            _stats.RemoveModifier(_stunStatHandle);
        }
        OnStunBegin();
        _stunStatHandle = _stats.AddModifier(new StatModifier(GameTags.StatusStun, MathOp.Added, 1));
        //_animationHandler?.SetAnimationState(CharacterAnimationState.Immobilized);

        _stunRoutine =  StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        _stunRoutine = null;
        if (IsDead)
            yield break;

        //_animationHandler?.SetAnimationState(CharacterAnimationState.Locomotion);
        _stats.RemoveModifier(_stunStatHandle);
        OnStunEnd();
    }

    protected virtual void OnDeath(IEntity killer) { }
    protected virtual void OnStunBegin(){}
    protected virtual void OnStunEnd(){}

    public virtual void ApplyDegen(DegenInfo degenInfo)
    {
        var damageInfo = new DamageInfo(degenInfo.Damage, degenInfo.Source, degenInfo.Position);

        if (!_damagePipeline.RunPreProcess(this, damageInfo, out float finalDamage))
        {
            Debug.Log($"[Entity:TakeDamage] Something logical prevented damage application.");
            return;
        }

        degenInfo.Damage = damageInfo.DamageMap;
        _degen.Apply(degenInfo);
    }

    public void RemoveDegen(Guid id)
    {
        _degen.RemoveDegen(id);
    }
}