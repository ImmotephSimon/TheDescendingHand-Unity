public enum StatType
{
    Health,
    MaxHealth,
    Damage,
    FireDamage,
    AttackSpeed,
    Armor,
    CritChance
}

public enum ModifierType
{
    Flat,
    AdditivePercent,
    Multiplicative
}

public readonly struct ModifierHandle
{
    public readonly int Id;

    public ModifierHandle(int id)
    {
        Id = id;
    }
}

public struct StatModifier
{
    public StatType Stat;
    public ModifierType Type;
    public float Value;

    public StatModifier(StatType stat, ModifierType type, float value)
    {
        Stat = stat;
        Type = type;
        Value = value;
    }
}