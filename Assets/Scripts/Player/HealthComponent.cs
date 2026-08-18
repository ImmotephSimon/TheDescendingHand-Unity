using System;
using UnityEngine;


public class HealthComponent : MonoBehaviour, IHealth
{
    private float _currentHealth;
    private IEntity owner;
    private IStatContainer stats;

    public event Action<float, float, bool> OnHealthChanged;

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    private float _maxHealth => stats.GetStat(GameTags.ModStatHealth);

    private void Awake()
    {
        owner = GetComponent<IEntity>();
        stats = GetComponent<IStatContainer>();
        if (stats == null) Debug.LogError($"{name} has no IStatContainer");

        stats.Listen(GameTags.ModStatHealth, OnMaxHealthChanged);
    }

    private void OnMaxHealthChanged(float maxHealth)
    {
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth, true);
    }

    private void Start()
    {
        _currentHealth = _maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth, true);
    }

    public void AdjustHealth(float healthAdjustment, object source, bool isInstant = true)
    {
        _currentHealth = Mathf.Clamp(
            _currentHealth + healthAdjustment,
            0,
            _maxHealth
        );
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth, isInstant);

        Debug.Log($"{owner} health now {_currentHealth} ({healthAdjustment:+#;-#;0} from {source ?? "Environment"})");

        if (_currentHealth <= 0)
            owner.Die(source as IEntity);
    }

    public void Restore(float amount)
    {
        _currentHealth = Mathf.Min(
            _currentHealth + amount,
            MaxHealth);

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth, true);
    }

}