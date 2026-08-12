using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyBrain))]
[RequireComponent(typeof(AbilityManager))]
[RequireComponent(typeof(Perception))]
public class Enemy : Entity, IExperienceSource
{
    [SerializeField] private ExperienceTable experienceTable;
    [SerializeField] private BalanceCurves balanceCurves;

    [SerializeField] private EnemyDefinition definition;
    [SerializeField] private Transform visualRoot;

    private float patrolRadius = 10f;

    private EnemyBrain brain;
    private Perception perception;
    private Rarity rarity;
    private int baseExperience = 1;
    private int _level = 1;
    private EnemyHealthBar _healthBar;
    public int ExperienceReward => experienceTable.ScaleByRarity(baseExperience, rarity);
    public event Action<Enemy> Died;

    public override Vector3 CursorPosition => perception.Target.position;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (experienceTable == null)
            Debug.LogWarning($"{name}: Missing ExperienceTable");

        if (balanceCurves == null)
            Debug.LogWarning($"{name}: Missing BalanceCurves");

        if (Application.isPlaying || definition == null || definition.ModelPrefab == null || visualRoot == null) return;

        // Ignore raw prefab assets in the project folder
        if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this)) return;

        UnityEditor.EditorApplication.delayCall += UpdateVisualPreview;
    }
    private void UpdateVisualPreview()
    {
        if (this == null || visualRoot == null || definition == null || definition.ModelPrefab == null) return;

        for (int i = visualRoot.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(visualRoot.GetChild(i).gameObject);
        }

        Instantiate(definition.ModelPrefab, visualRoot);
    }
#endif

    protected override void Awake()
    {
        base.Awake();

        brain = GetComponent<EnemyBrain>();
        perception = GetComponent<Perception>();
    }
    private void Start()
    {
        _healthBar = GetComponentInChildren<EnemyHealthBar>();
        _healthBar.Bind(GetComponent<IHealth>());

        GetComponent<DropsComponent>().DropAtLocation();
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
        animationHandler = GetComponentInChildren<IAnimationHandler>();
        stats = GetComponent<IStatContainer>();

        RegisterActions();
        ApplyBaseStats();
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
        stats.AddModifier(
            new StatModifier(
                GameTags.ModStatHealth, 
                MathOp.Added, 
                20));
        stats.AddModifier(
            new StatModifier(
                GameTags.ModOffenseDamage, 
                MathOp.Added, 
                balanceCurves.ExpectedPlayerLife.Evaluate(_level) * 0.1f));
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
        GetComponent<DropsComponent>().DropAtLocation();

        Died?.Invoke(this);
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

        if (entity.HostileLayer != TeamLayer)
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