using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyBrain))]
[RequireComponent(typeof(AbilityManager))]
[RequireComponent(typeof(Perception))]
public class Enemy : Entity, IExperienceSource
{
    [SerializeField] private ExperienceTable experienceTable;
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private int level = 1;
    [SerializeField] private BalanceCurves balanceCurves;

    private EnemyBrain brain;
    private Perception perception;
    private Rarity rarity;
    private int baseExperience = 1;
    private EnemyHealthBar _healthBar;

    public int ExperienceReward => experienceTable.ScaleByRarity(baseExperience, rarity);

    private void OnValidate()
    {
        if (experienceTable == null)
            Debug.LogWarning($"{name}: Missing ExperienceTable");

        if (balanceCurves == null)
            Debug.LogWarning($"{name}: Missing BalanceCurves");
    }

    protected override void Awake()
    {
        base.Awake();

        brain = GetComponent<EnemyBrain>();
        perception = GetComponent<Perception>();
        animationHandler = GetComponentInChildren<IAnimationHandler>();
        stats = GetComponent<IStatContainer>();
        if (animationHandler == null) Debug.LogError("Enemy has no animation handler");
        if (stats == null) Debug.LogError("Enemy has no stat container");
        RegisterActions();
        ApplyBaseStats();
    }
    private void Start()
    {
        _healthBar = GetComponentInChildren<EnemyHealthBar>();
        _healthBar.Bind(GetComponent<IHealth>());
    }

    private void ApplyBaseStats()
    {
        stats.AddModifier(
            new StatModifier(
                GameTags.ModStatHealth, 
                MathOp.Flat, 
                20));
        stats.AddModifier(
            new StatModifier(
                GameTags.ModOffenseDamage, 
                MathOp.Flat, 
                balanceCurves.ExpectedPlayerLife.Evaluate(level) * 0.1f));
    }

    private void RegisterActions()
    {
        brain.AddAction(new ChaseAction(this));
        brain.AddAction(new PatrolAction(this, patrolRadius));
        foreach (var ability in GetComponents<IEnemyAttack>())
        {
            brain.AddAction(new AttackAction(this, ability));
        }
    }


    protected override void OnDeath(IEntity killer)
    {
        brain.SetState(BrainState.Dead);
        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;
        Destroy(_healthBar.gameObject);

    }

    protected override void OnEntityDied(IEntity victim, IEntity killer)
    {
        if (victim.TeamLayer != TeamLayer)
        {
            Debug.Log($"Disable AI on {killer}");
            perception.SetTargetValidity(victim.Transform, false);
        }
    }
    protected override void OnEntityRevived(IEntity entity)
    {
        base.OnEntityRevived(entity);

        if (entity.TeamLayer != TeamLayer)
        {
            Debug.Log($"Enable AI on {entity} revival");
            perception.SetTargetValidity(entity.Transform, true);
        }
    }
    protected override void OnStunBegin()
    {
        brain.SetState(BrainState.Suspended);
    }

    protected override void OnStunEnd()
    {
        brain.SetState(BrainState.Active);
    }

}