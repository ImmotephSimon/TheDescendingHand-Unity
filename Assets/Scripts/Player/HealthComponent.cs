using System;
using UnityEngine;


public class HealthComponent : MonoBehaviour, IHealth
{
    [SerializeField] private GameTag LifeTag;

    private IEntity owner;
    private IStatContainer stats;
    private float _currentHealth;

    public event Action OnHealthChanged;

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    private float _maxHealth =>
        stats.GetStat(
            LifeTag,
            new TagContainer());


    private void Awake()
    {
        owner = GetComponent<IEntity>();
        stats = GetComponent<IStatContainer>();
        if (stats == null) Debug.LogError($"{name} has no IStatContainer");
    }
    private void Start()
    {
        _currentHealth = MaxHealth;
        OnHealthChanged?.Invoke();
    }

    public void AdjustHealth(float healthAdjustment)
    {
        _currentHealth -= healthAdjustment;
        OnHealthChanged?.Invoke();

        if (_currentHealth <= 0)
            owner.Die();
    }

    public void Restore(float amount)
    {
        _currentHealth = Mathf.Min(
            _currentHealth + amount,
            MaxHealth);

        OnHealthChanged?.Invoke();
    }

}