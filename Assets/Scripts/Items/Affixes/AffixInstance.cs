using System;
using UnityEngine;

public class AffixInstance
{
    public AffixDefinition Definition;
    public float Tier = 1f;
    public float Value
    {
        get
        {
            if (Definition == null || Definition.BaseValue == 0f) return 0f;

            float scaled = Definition.BaseValue * Tier;

            if (Definition.MathOp == MathOp.Added)
            {
                int sign = Math.Sign(Definition.BaseValue);
                return sign * Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(scaled)));
            }

            if (Definition.MathOp == MathOp.Multiplicative)
            {
                if (Definition.BaseValue > 1f) return Mathf.Max(1.01f, scaled);
                if (Definition.BaseValue < 1f) return Mathf.Min(0.99f, scaled);
                return scaled;
            }

            // Additive (%) whole number or float scale
            float minMagnitude = Mathf.Abs(Definition.BaseValue) >= 1f ? 1f : 0.01f;
            return Math.Sign(Definition.BaseValue) * Mathf.Max(minMagnitude, Mathf.Abs(scaled));
        }
    }

    public StatModifier ToStatModifier()
    {

        return new StatModifier(
            Definition.Modifier,
            Definition.MathOp,
            Value,
            Definition.TagRequirement);
    }

    public string GetDisplayText()
    {
        if (!string.IsNullOrWhiteSpace(Definition.NameOverride))
            return $"{Definition.NameOverride}: {Value}";

        return ToStatModifier().ToString();
    }
}