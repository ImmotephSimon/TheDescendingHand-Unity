using System;

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

public readonly struct StatModifier
{
    public readonly GameTag Stat;
    public readonly ModifierType Type;
    public readonly float Value;
    public readonly TagContainer RequiredTags;

    public StatModifier(GameTag stat, ModifierType type, float value, TagContainer requiredTags)
    {
        Stat = stat;
        Type = type;
        Value = value;
        RequiredTags = requiredTags;
    }
    public StatModifier(GameTag stat, ModifierType type, float value)
        : this(stat, type, value, new TagContainer()) { }
}