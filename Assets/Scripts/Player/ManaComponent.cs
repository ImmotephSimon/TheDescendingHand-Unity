using System;
using UnityEngine;

public class ManaComponent : MonoBehaviour, IMana
{
    [SerializeField] private float maxMana = 20f;

    private float currentMana;

    public float CurrentMana => currentMana;
    public float MaxMana => maxMana;

    public event Action OnManaChanged;

    private void Start()
    {
        currentMana = maxMana;
        OnManaChanged?.Invoke();
    }

    public void Spend(float amount)
    {
        currentMana = Mathf.Max(currentMana - amount, 0f);
        OnManaChanged?.Invoke();
    }

    public void Restore(float amount)
    {
        currentMana = Mathf.Min(currentMana + amount, maxMana);
        OnManaChanged?.Invoke();
    }
}