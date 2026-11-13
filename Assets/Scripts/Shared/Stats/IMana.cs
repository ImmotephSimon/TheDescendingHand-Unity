using System;

public interface IMana
{
    float CurrentMana { get; }
    float MaxMana { get; }
    float ManaPerSecond { get; }

    event Action<float, float, bool> OnManaChanged; // current, max, isInstant
    void SetManaPerSecond(float manaPerSecond);
}