using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyBrain))]
[RequireComponent(typeof(AbilityManager))]
[RequireComponent(typeof(Perception))]
[RequireComponent(typeof(EnemyMovementController))]
public class ServerEnemy : Entity, IExperienceSource
{
    [SerializeField] private bool debug = false;
    [SerializeField] private ExperienceTable experienceTable;
    [SerializeField] private BalanceCurves balanceCurves;

    [SerializeField] private EnemyDefinition definition;
    [SerializeField] private Transform visualRoot;

    private float patrolRadius = 10f;

    private EnemyBrain _brain;
    private Perception _perception;
    private EnemyStatsNetwork _network;
    private IHealth _health;
    private Rarity rarity;
    private int baseExperience = 1;
    private int _level = 1;

    public EnemyType EnemyType => definition.EnemyType;
    public int ExperienceReward => experienceTable.ScaleByRarity(baseExperience, rarity);

    public override Vector3 CursorPosition => _perception.Target.position;
    public HitBoxSampleData HitBoxSampleData => definition.HitBoxSampleData;

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

        _brain = GetComponent<EnemyBrain>();
        _perception = GetComponent<Perception>();
        _network = GetComponent<EnemyStatsNetwork>();
        _health = GetComponent<IHealth>();
        _health.OnHealthChanged += SyncHealth;

        if (debug) Initialize(definition, 1);
    }

    public void Initialize(EnemyDefinition definition, int enemyLevel)
    {
        this.definition = definition;
        _level = enemyLevel;

        // Runtime visual setup
        //if (definition.WorldModel != null && visualRoot.childCount == 0)
        //{
        //    Instantiate(definition.WorldModel, visualRoot);
        //}

        AttachAttacks(definition.Attacks);
        _network.EnemyType.Value = definition.EnemyType;
        _network.WorldModelId.Value = definition.Id;

        _brain = GetComponent<EnemyBrain>();
        _perception = GetComponent<Perception>();

        RegisterActions();
        ApplyBaseStats();
    }

    private void SyncHealth(float current, float max)
    {
        _network.HealthRate.Value = _health.HealthPerSecond;
        _network.MaxHealth.Value = max;
        _network.Health.Value = current;
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
                GameTags.ModUtilityDuration,
                MathOp.Added,
                1f,
                GameTags.StatusStun.ToRequirement()));


    }

    private void RegisterActions()
    {
        _brain.AddAction(new ChaseAction(this));
        _brain.AddAction(new PatrolAction(this, patrolRadius));
        foreach (var ability in GetComponents<IEnemyAttack>())
        {
            _brain.AddAction(new AttackAction(this, ability));
        }
    }


    protected override void OnDeath(IEntity killer)
    {
        _brain.SetState(BrainState.Dead);

        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        if (definition.EnemyType == EnemyType.Unique)
        {
            killer.Transform.GetComponent<PlayerNetworkActions>().HideBossBar();
        }

        GetComponent<DropsComponent>().DropFromEnemy(transform.position);
    }

    protected override void OnEntityDied(IEntity victim, IEntity killer)
    {
        if (victim.HostileLayer == TeamLayer)
        {
            Debug.Log($"Disable AI on {killer}");
            _perception.SetTargetValidity(victim.Transform, false);
        }
    }
    protected override void OnEntityRevived(IEntity entity)
    {
        base.OnEntityRevived(entity);

        if (entity.HostileLayer == TeamLayer)
        {
            Debug.Log($"Enable AI on {entity} revival");
            _perception.SetTargetValidity(entity.Transform, true);
        }
    }
    protected override void OnStunBegin()
    {
        _brain.SetState(BrainState.Suspended);
    }

    protected override void OnStunEnd()
    {
        _brain.SetState(BrainState.Active);
    }

}