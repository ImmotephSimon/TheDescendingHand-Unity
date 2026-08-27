using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class StatModifierComponent : MonoBehaviour, IStatContainer, ICalculator
{

    private readonly Dictionary<GameTag, Dictionary<int, StatModifier>> modifiers = new();
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

    public ModifierHandle AddModifier(StatModifier modifier, float duration = 0)
    {
        if (modifier.Stat == null)
        {
            Debug.LogError("Tried to add a null StatModifier.");
            return default;
        }

        int id = nextId++;

        if (!modifiers.TryGetValue(modifier.Stat, out var statModifiers))
        {
            statModifiers = new Dictionary<int, StatModifier>();
            modifiers[modifier.Stat] = statModifiers;
        }

        statModifiers.Add(id, modifier);
        NotifyStatChanged(modifier.Stat);

        ModifierHandle handle = new(id, modifier.Stat);

        if (duration > 0)
            StartCoroutine(RemoveModifierAfter(handle, duration));


        return handle;
    }

    private IEnumerator RemoveModifierAfter(ModifierHandle handle, float duration)
    {
        yield return new WaitForSeconds(duration);
        RemoveModifier(handle, true);
    }

    public void RemoveModifier(GameTag stat)
    {
        if (!modifiers.TryGetValue(stat, out _))
        {
            Debug.LogError($"No modifiers found for stat {stat}.");
            return;
        }

        modifiers.Remove(stat);
        NotifyStatChanged(stat);
    }

    public void RemoveModifier(ModifierHandle handle, bool allowPrematureRemoval = false)
    {
        var stat = handle.Stat;

        if (!modifiers.TryGetValue(stat, out var statModifiers))
        {
            if (allowPrematureRemoval)
            {
                handle.Invalidate();
                return;
            }

            Debug.LogError($"No modifiers found for stat {stat}.");
            return;
        }

        if (!statModifiers.Remove(handle.Id))
        {
            if (allowPrematureRemoval)
            {
                handle.Invalidate();
                return;
            }

            Debug.LogError($"Attempted to remove unknown modifier handle {handle.Id}.");
            return;
        }

        if (statModifiers.Count == 0)
            modifiers.Remove(stat);

        NotifyStatChanged(stat);
        handle.Invalidate();
    }


    public float GetStat(GameTag stat, TagContainer context, float baseValue = 0)
    {
        if (stat == null)
            return 0;

        float value = baseValue;
        float additive = 0;
        float multiplier = 1;

        if (!modifiers.TryGetValue(stat, out var statModifiers))
            return value;

        foreach (var modifier in statModifiers.Values)
        {
            if (!modifier.RequiredTags.IsSatisfiedBy(context))
                continue;

            switch (modifier.Op)
            {
                case MathOp.Set:
                    return modifier.Value;

                case MathOp.Added:
                    value += modifier.Value;
                    break;

                case MathOp.Additive:
                    additive += modifier.Value;
                    break;

                case MathOp.Multiplicative:
                    multiplier *= modifier.Value;
                    break;
            }
        }

        return value * (1 + additive) * multiplier;
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
        if (!IsSelected())
            return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 500));
        GUILayout.Label($"<b>Modifiers on {gameObject.name}</b>");

        foreach (var group in modifiers)
        {
            GUILayout.Label($"<b>{group.Key}</b>");

            foreach (var mod in group.Value.Values)
            {
                GUILayout.Label($"  {mod.Op} {mod.Value}");
            }
        }

        GUILayout.EndArea();
    }

    private bool IsSelected()
    {
#if UNITY_EDITOR
        return UnityEditor.Selection.activeGameObject == gameObject;
#else
    return false;
#endif
    }

}