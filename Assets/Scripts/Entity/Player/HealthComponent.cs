using System;
using UnityEngine;


public class HealthComponent : MonoBehaviour, IHealth
{
    [SerializeField] private GameTag LifeTag;

    private float _currentHealth;
    private IEntity owner;
    private IStatContainer stats;

    public event Action<float, float> OnHealthChanged;

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
        _currentHealth = _maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public void AdjustHealth(float healthAdjustment, IEntity source)
    {
        _currentHealth += healthAdjustment;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        Debug.Log($"{owner} health now {_currentHealth} (-{healthAdjustment} from {source})");

        if (_currentHealth <= 0)
            owner.Die(source);
    }

    public void Restore(float amount)
    {
        _currentHealth = Mathf.Min(
            _currentHealth + amount,
            MaxHealth);

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

}