using System;
using UnityEngine;


public class HealthComponent : MonoBehaviour
{
    [SerializeField] private GameTag LifeTag;

    private IEntity owner;
    private IStatContainer stats;
    private float currentHealth;

    
    private void Awake()
    {
        owner = GetComponent<IEntity>();
        stats = GetComponent<IStatContainer>();
        if (stats == null) Debug.LogError($"{name} has no IStatContainer");
    }

    private void Start()
    {
        currentHealth = MaxHealth;
    }

    public float CurrentHealth => currentHealth;

    public float MaxHealth =>
        stats.GetStat(
            LifeTag,
            new TagContainer());

    public void AdjustHealth(float healthAdjustment)
    {
        currentHealth -= healthAdjustment;

        if (currentHealth <= 0)
            owner.Die();
    }

    public void Restore(float amount)
    {
        currentHealth = Mathf.Min(
            currentHealth + amount,
            MaxHealth);
    }

}