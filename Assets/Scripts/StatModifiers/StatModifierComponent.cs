using System.Collections.Generic;
using UnityEngine;

public class StatModifierComponent : MonoBehaviour
{
    private readonly Dictionary<int, StatModifier> modifiers = new();

    private int nextId = 0;

    public ModifierHandle AddModifier(StatModifier modifier)
    {
        int id = nextId++;
        modifiers.Add(id, modifier);

        return new ModifierHandle(id);
    }

    public void RemoveModifier(ModifierHandle handle)
    {
        modifiers.Remove(handle.Id);
    }

    public float GetStat(StatType stat, float baseValue)
    {
        float value = baseValue;
        float additive = 0;
        float multiplier = 1;

        foreach (var modifier in modifiers.Values)
        {
            if (modifier.Stat != stat)
                continue;

            switch (modifier.Type)
            {
                case ModifierType.Flat:
                    value += modifier.Value;
                    break;

                case ModifierType.AdditivePercent:
                    additive += modifier.Value;
                    break;

                case ModifierType.Multiplicative:
                    multiplier *= modifier.Value;
                    break;
            }
        }

        value *= 1 + additive;
        value *= multiplier;

        return value;
    }
}