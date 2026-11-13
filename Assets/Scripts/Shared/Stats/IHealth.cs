using System;

public interface IHealth
{
    float CurrentHealth { get; }
    float MaxHealth { get; }
    float HealthPerSecond { get; }

    event Action<float, float> OnHealthChanged;

    void AdjustHealth(float finalDamage, object source);
    void SetHealthPerSecond(float healthPerSecond);
}