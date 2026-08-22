using System;
using Unity.AppUI.UI;
using UnityEngine;

public enum MathOp
{
    Added,
    Additive,
    Multiplicative,
    Set
}

public sealed class ModifierHandle
{
    public int Id { get; private set; }
    public bool IsValid => Id != -1;

    public ModifierHandle(int id)
    {
        Id = id;
    }

    public void Invalidate()
    {
        Id = -1;
    }

}
[Serializable]
public struct StatModifier
{
    public GameTag Stat;
    public MathOp Op;
    public float Value;
    public TagRequirement RequiredTags;

    public StatModifier(GameTag stat, MathOp type, float value, TagRequirement requiredTags)
    {
        Stat = stat;
        Op = type;
        Value = value;
        RequiredTags = requiredTags;
    }

    public StatModifier(GameTag stat, MathOp type, float value)
        : this(stat, type, value, TagRequirement.Empty) { }


    /// <summary>
    /// Sets a binary state modifier.
    /// </summary>
    public StatModifier(GameTag stat)
    : this(stat, MathOp.Set, 1f, TagRequirement.Empty) { }

    public override string ToString()
    {
        string statName = Stat != null ? Stat.ToString() : "Unknown";
        string reqStr = RequiredTags.IsElemental
            ? "Elemental"
            : !RequiredTags.IsEmpty
                ? RequiredTags.ToString()
                : "";

        string requirement = !string.IsNullOrEmpty(reqStr) ? $"{reqStr} " : "";

        if (Op == MathOp.Added)
        {
            int roundVal = Mathf.RoundToInt(Value);
            return $"{requirement}{statName}: {(roundVal >= 0 ? "+" : "-")}{Mathf.Abs(roundVal)}";
        }

        if (Op == MathOp.Additive)
        {
            int roundVal = Mathf.RoundToInt(Value * 100f);
            string prefix = roundVal >= 0 ? "Increased " : "Decreased ";
            return $"{prefix}{requirement}{statName}: {Mathf.Abs(roundVal)}%";
        }

        if (Op == MathOp.Multiplicative)
        {
            float percentChange = (Value - 1f) * 100f;
            int roundVal = Mathf.RoundToInt(percentChange);
            string prefix = roundVal >= 0 ? "More " : "Less ";
            return $"{prefix}{requirement}{statName}: {Mathf.Abs(roundVal)}%";
        }

        return $"{requirement}{statName}: {Mathf.RoundToInt(Value)}";
    }
}