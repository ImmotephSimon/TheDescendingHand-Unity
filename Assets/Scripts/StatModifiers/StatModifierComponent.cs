using System;
using System.Collections.Generic;
using UnityEngine;

public class StatModifierComponent : MonoBehaviour, IStatContainer, ICalculator
{

    private readonly Dictionary<int, StatModifier> modifiers = new();
    private readonly Dictionary<GameTag, Action<float>> listeners = new();
    private int nextId = 0;

    public void Listen(GameTag stat, Action<float> callback)
    {
        if (stat == null || callback == null) return;

        if (!listeners.ContainsKey(stat))
            listeners[stat] = null;

        listeners[stat] += callback;
    }

    public void StopListening(GameTag stat, Action<float> callback)
    {
        if (stat == null || callback == null) return;

        if (listeners.ContainsKey(stat))
        {
            listeners[stat] -= callback;
            if (listeners[stat] == null)
                listeners.Remove(stat);
        }
    }

    public void Listen(GameTag[] stats, Action<float> callback)
    {
        foreach (var stat in stats)
            Listen(stat, callback);
    }

    public void StopListening(GameTag[] stats, Action<float> callback)
    {
        foreach (var stat in stats)
            StopListening(stat, callback);
    }

    private void NotifyStatChanged(GameTag stat)
    {
        if (stat == null) return;

        if (listeners.TryGetValue(stat, out var callback) && callback != null)
        {
            float newValue = GetStat(stat);
            callback.Invoke(newValue);
        }
    }

    public ModifierHandle AddModifier(StatModifier modifier)
    {
        if (modifier.Stat == null)
        {
            Debug.LogError("Tried to add a null StatModifier.");
            return default;
        }

        int id = nextId++;
        modifiers.Add(id, modifier);
        NotifyStatChanged(modifier.Stat);
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
        handle.Invalidate();
        NotifyStatChanged(modifier.Stat);
    }

    public float GetStat(GameTag stat, TagContainer context, float baseValue = 0)
    {
        if (stat == null) return 0;

        float value = baseValue;
        float additive = 0;
        float multiplier = 1;

        foreach (var modifier in modifiers.Values)
        {
            if (!modifier.Stat.Equals(stat))
                continue;

            if (!modifier.RequiredTags.IsSatisfiedBy(context))
                continue;

            switch (modifier.Op)
            {
                case MathOp.Set: value = modifier.Value; break;
                case MathOp.Added: value += modifier.Value; break;
                case MathOp.Additive: additive += modifier.Value; break;
                case MathOp.Multiplicative: multiplier *= modifier.Value; break;
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
        return GetStat(stat, TagContainer.Empty, 0);
    }

    public Dictionary<GameTag, float> CalculateDamage(TagContainer tags, float effectiveness, GameTag damageConversion)
    {
        Dictionary<GameTag, float> _damage = new();

        foreach (GameTag damageType in GameTags.DamageTypes)
        {
            var baseDamage = GetStat(
                GameTags.ModOffenseDamage,
                tags.With(damageType)
            );

            _damage.Add(damageType, baseDamage * effectiveness);
        }
        Debug.Log("ignoring damage conversion atm");
        return _damage;
    }


    private void OnGUI()
    {
        if (!CompareTag("Player"))
            return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 500));
        GUILayout.Label($"<b>Modifiers on {gameObject.name}</b>");

        foreach (var mod in modifiers.Values)
        {
            GUILayout.Label($"{mod.Stat}: {mod.Op} {mod.Value}");
        }

        GUILayout.EndArea();
    }

}