using UnityEngine;

public class GlobeDrop : WorldDrop
{
    private float _healPercentage;

    public void Initialize(float healPercentage)
    {
        _healPercentage = healPercentage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerStarted)
            return;

        if (!other.TryGetComponent<IHealth>(out var health))
            return;

        health.AdjustHealth(health.MaxHealth * _healPercentage, this);
        Despawn();
    }
}