using System;
using UnityEngine;

public class ManaComponent : MonoBehaviour, IMana
{
    private IStatContainer stats;
    private float _maxMana => stats.GetStat(GameTags.ModStatMana);
    private float _currentMana;

    public float CurrentMana => _currentMana;
    public float MaxMana => _maxMana;

    public event Action<float, float> OnManaChanged;

    private void Start()
    {
        stats = GetComponent<IStatContainer>();
        _currentMana = _maxMana;
        OnManaChanged?.Invoke(_currentMana, _maxMana);
    }

    public void Spend(float amount)
    {
        _currentMana = Mathf.Max(_currentMana - amount, 0f);
        OnManaChanged?.Invoke(_currentMana, _maxMana);
    }

    public void Restore(float amount)
    {
        _currentMana = Mathf.Min(_currentMana + amount, _maxMana);
        OnManaChanged?.Invoke(_currentMana, _maxMana);
    }
}