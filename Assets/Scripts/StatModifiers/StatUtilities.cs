using System;
using UnityEngine;

public enum MathOp
{
    Added,
    Additive,
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

[Serializable]
public struct StatModifier
{
    public GameTag Stat;
    public MathOp Op;
    public float Value;
    public TagContainer RequiredTags;
    [HideInInspector] public ModifierSource Source;

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