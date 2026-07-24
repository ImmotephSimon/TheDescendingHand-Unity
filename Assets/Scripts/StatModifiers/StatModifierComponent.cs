using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class StatModifierComponent : MonoBehaviour, IStatContainer
{
    [SerializeField] private TagRestriction[] damageTypes;

    private readonly Dictionary<int, StatModifier> modifiers = new();
    private int nextId = 0;
    GameTag _damageStat = new("Mod.Offense.Damage");
    public event Action<GameTag> OnStatChanged;

    public ModifierHandle AddModifier(StatModifier modifier)
    {
        if (modifier.Stat == null)
        {
            Debug.LogError("Tried to add a null StatModifier.");
            return default;
        }

        int id = nextId++;
        modifiers.Add(id, modifier);
        OnStatChanged?.Invoke(modifier.Stat);
        return new ModifierHandle(id);
    }

    public void RemoveModifier(ModifierHandle handle)
    {
        if (!modifiers.TryGetValue(handle.Id, out var modifier))
        {
            Debug.LogError($"Attempted to remove unknown modifier handle {handle.Id}.");
            return;
        }

        modifiers.Remove(handle.Id);
        OnStatChanged?.Invoke(modifier.Stat);
    }

    public float GetStat(GameTag stat, TagContainer context, float baseValue = 0)
    {
        if (stat == null) return 0;

        float value = baseValue;
        float additive = 0;
        float multiplier = 1;

        foreach (var modifier in modifiers.Values)
        {
            // modifier.Stat should be a GameTag
            if (!modifier.Stat.Equals(stat))
                continue;

            if (modifier.RequiredTags != null && !context.HasAll(modifier.RequiredTags))
                continue;

            switch (modifier.Type)
            {
                case ModifierType.Flat: value += modifier.Value; break;
                case ModifierType.AdditivePercent: additive += modifier.Value; break;
                case ModifierType.Multiplicative: multiplier *= modifier.Value; break;
            }
        }

        return value * (1 + additive) * multiplier;
    }

    public float GetStat(GameTag stat, TagContainer context)
    {
        return GetStat(stat, context, 0);
    }

    public float GetStat(GameTag stat)
    {
        return GetStat(stat, new TagContainer(), 0);
    }

    private void OnGUI()
    {
        if (!CompareTag("Player"))
            return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 500));
        GUILayout.Label($"<b>Modifiers on {gameObject.name}</b>");

        foreach (var mod in modifiers.Values)
        {
            GUILayout.Label($"{mod.Stat}: {mod.Type} {mod.Value}");
        }

        GUILayout.EndArea();
    }
    public Dictionary<TagRestriction, float> GetDamageRatios(TagContainer context)
    {
        var map = new Dictionary<TagRestriction, float>(damageTypes.Length);

        for (int i = 0; i < damageTypes.Length; i++)
        {
            var type = damageTypes[i];
            map[type] = GetStat(_damageStat, context.With(type.Tags.PrimaryTag));
        }

        return map;
    }
}