using System;

public interface IHealth
{
    float CurrentHealth { get; }
    float MaxHealth { get; }

    event Action<float, float> OnHealthChanged; // current, max

    void AdjustHealth(float finalDamage, IEntity source);
}