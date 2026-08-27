using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyBrain))]
[RequireComponent(typeof(AbilityManager))]
[RequireComponent(typeof(Perception))]
public class Enemy : Entity, IExperienceSource
{
    [SerializeField] private bool debug = false;
    [SerializeField] private ExperienceTable experienceTable;
    [SerializeField] private BalanceCurves balanceCurves;

    [SerializeField] private EnemyDefinition definition;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private GameObject healthBarPrefab;

    private float patrolRadius = 10f;

    private EnemyBrain brain;
    private Perception perception;
    private Rarity rarity;
    private int baseExperience = 1;
    private int _level = 1;
    private IHealthBar _healthBar;
    public int ExperienceReward => experienceTable.ScaleByRarity(baseExperience, rarity);

    public override Vector3 CursorPosition => perception.Target.position;


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

        if (debug) Initialize(definition, 1);
    }

    public void Initialize(EnemyDefinition definition, int enemyLevel)
    {
        this.definition = definition;
        _level = enemyLevel;

        // Runtime visual setup
        if (definition.ModelPrefab != null && visualRoot.childCount == 0)
        {
            Instantiate(definition.ModelPrefab, visualRoot);
        }

        AttachAttacks(definition.Attacks);

        brain = GetComponent<EnemyBrain>();
        perception = GetComponent<Perception>();
        _animationHandler = GetComponentInChildren<IAnimationHandler>();
        Debug.Assert(_animationHandler != null, $"Failed to find animation handler.");

        RegisterActions();
        ApplyBaseStats();
    }

    protected override void Start()
    {
        base.Start();
        if (definition.EnemyType == EnemyType.Unique)
        {
            ClientBridge.Instance.OnClientPlayerReady += BindBossHealthBar;
        }
        else
        {
            GameObject barObj = Instantiate(healthBarPrefab, transform);
            _healthBar = barObj.GetComponent<EnemyHealthBar>();
            _healthBar.Bind(GetComponent<IHealth>());
        }
    }

    private void BindBossHealthBar(ClientPlayer _)
    {
        ClientBridge.Instance.OnClientPlayerReady -= BindBossHealthBar;
        _healthBar = ClientBridge.Instance.PlayerHUD.BindBossHealthBar(this);
    }

    private void AttachAttacks(List<EnemyAttackDefinition> attackDefinitions)
    {
        if (attackDefinitions == null) return;

        foreach (var definition in attackDefinitions)
        {
            if (definition != null)
            {
                definition.Create(this);
            }
        }
    }

    private void ApplyBaseStats()
    {
        _stats.AddModifier(
            new StatModifier(
                GameTags.ModStatHealth, 
                MathOp.Added, 
                20));
        _stats.AddModifier(
            new StatModifier(
                GameTags.ModOffenseDamage, 
                MathOp.Added, 
                balanceCurves.ExpectedPlayerLife.Evaluate(_level) * 0.1f));
        _stats.AddModifier(
            new StatModifier(
                GameTags.ModStatStunThreshold,
                MathOp.Added,
                0.2f));
        _stats.AddModifier(
            new StatModifier(
                GameTags.ModStatStunDuration,
                MathOp.Added,
                1f));


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
        if (_healthBar is Component healthBarComponent) 
            Destroy(healthBarComponent.gameObject);

        GetComponent<DropsComponent>().DropFromEnemy(transform.position);
    }

    protected override void OnEntityDied(IEntity victim, IEntity killer)
    {
        if (victim.HostileLayer == TeamLayer)
        {
            Debug.Log($"Disable AI on {killer}");
            perception.SetTargetValidity(victim.Transform, false);
        }
    }
    protected override void OnEntityRevived(IEntity entity)
    {
        base.OnEntityRevived(entity);

        if (entity.HostileLayer == TeamLayer)
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