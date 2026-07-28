using System;
using static Unity.VisualScripting.Member;

public enum MathOp
{
    Flat,
    AdditivePercent,
    Multiplicative
}
public enum ModifierSource
{
    Implicit,
    Explicit
}

public readonly struct ModifierHandle
{
    public readonly int Id;
    public bool IsValid => Id != 0;
    public ModifierHandle(int id)
    {
        Id = id;
    }
}

public readonly struct StatModifier
{
    public readonly GameTag Stat;
    public readonly MathOp Op;
    public readonly float Value;
    public readonly TagContainer RequiredTags;
    public readonly ModifierSource Source;

    public StatModifier(GameTag stat, MathOp type, float value, TagContainer requiredTags, ModifierSource source)
    {
        Stat = stat;
        Op = type;
        Value = value;
        RequiredTags = requiredTags;
        Source = source;
    }
    public StatModifier(GameTag stat, MathOp type, float value)
        : this(stat, type, value, TagContainer.Empty, ModifierSource.Explicit) { }
}