using System;
using UnityEngine;

public class ManaComponent : MonoBehaviour, IMana
{
    private IStatContainer stats;
    private float _maxMana => stats.GetStat(GameTags.ModStatMana);
    private float _currentMana;

    public float CurrentMana => _currentMana;
    public float MaxMana => _maxMana;

    public event Action<float, float, bool> OnManaChanged;

    private void Awake()
    {
        stats = GetComponent<IStatContainer>();
    }

    private void Start()
    {
        
        _currentMana = _maxMana;
        OnManaChanged?.Invoke(_currentMana, _maxMana, true);

        stats.Listen(GameTags.ModStatMana, OnMaxManaChanged);
    }

    private void OnMaxManaChanged(float maxMana)
    {
        OnManaChanged?.Invoke(_currentMana, _maxMana, true);
    }

    public void Spend(float amount)
    {
        _currentMana = Mathf.Max(_currentMana - amount, 0f);
        OnManaChanged?.Invoke(_currentMana, _maxMana, true);
    }

    public void Restore(float amount, bool isInstant)
    {
        _currentMana = Mathf.Min(_currentMana + amount, _maxMana);
        OnManaChanged?.Invoke(_currentMana, _maxMana, isInstant);
    }
}