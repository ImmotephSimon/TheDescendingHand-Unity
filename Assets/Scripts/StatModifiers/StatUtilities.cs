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

    public override string ToString()
    {
        bool isPercent = Op == MathOp.Additive || Op == MathOp.Multiplicative;
        float displayValue = isPercent ? Value * 100f : Value;

        int roundedValue = Mathf.RoundToInt(displayValue);
        int absValue = Mathf.Abs(roundedValue);

        string statName = Stat != null ? Stat.ToString() : "Unknown";
        string tagPrefix = (RequiredTags != null && !RequiredTags.IsEmpty) ? $"{RequiredTags} " : "";

        return Op switch
        {
            MathOp.Added => $"{tagPrefix}{statName}: {(roundedValue >= 0 ? "+" : "-")}{absValue}",
            MathOp.Additive => $"{tagPrefix}{(roundedValue >= 0 ? "Increased" : "Decreased")} {statName}: {absValue}%",
            MathOp.Multiplicative => $"{tagPrefix}{(roundedValue >= 0 ? "More" : "Less")} {statName}: {absValue}%",
            _ => $"{tagPrefix}{statName}: {roundedValue}"
        };
    }
}