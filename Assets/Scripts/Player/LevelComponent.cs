using System;
using UnityEngine;

public class LevelComponent : MonoBehaviour
{
    [SerializeField] private BalanceCurves balanceCurves;
    [SerializeField] private int level = 1;

    public int Level => level;
    public float Progress => (float)_experience / _levelExperience;

    private IStatContainer stats;
    private IEntity owner;
    private ModifierHandle _modifierHandle;

    private int _experience;
    private int _levelExperience = 20;

    public event Action<int, float> OnExperienceChanged;

    private void Awake()
    {
        stats = GetComponent<IStatContainer>();
        owner = GetComponent<IEntity>();

        ApplyLevelStats();
    }

    private void Start()
    {
        GameWorld.Instance.EntityDied += OnEntityDied;
    }

    private void OnDestroy()
    {
        if (GameWorld.Instance != null)
            GameWorld.Instance.EntityDied -= OnEntityDied;
    }

    private void OnEntityDied(IEntity victim, IEntity killer)
    {
        if (killer == owner && victim is IExperienceSource source)
        {
            AddExperience(source.ExperienceReward);
        }
    }

    private void AddExperience(int amount)
    {
        _experience += amount;

        if (_experience >= _levelExperience)
        {
            _experience -= _levelExperience;
            level++;

            UpdateLevelExperienceRequirement();
            ApplyLevelStats();
        }

        OnExperienceChanged?.Invoke(level, Progress);
    }

    private void UpdateLevelExperienceRequirement()
    {
        _levelExperience = Mathf.RoundToInt(_levelExperience * 1.1f);
    }

    public void LevelUp()
    {
        level++;
        _experience = 0;

        ApplyLevelStats();
    }

    private void ApplyLevelStats()
    {
        if (_modifierHandle?.IsValid == true)
            stats.RemoveModifier(_modifierHandle);

        _modifierHandle = stats.AddModifier(
            new StatModifier(
                GameTags.ModStatHealth,
                MathOp.Added,
                balanceCurves.ExpectedPlayerLife.Evaluate(level)
            ));
    }
}