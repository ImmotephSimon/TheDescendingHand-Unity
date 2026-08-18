using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStatModifierData", menuName = "Stats/Stat Modifier Data")]
public class StatModifierData : ScriptableObject
{
    public List<StatModifier> Modifiers = new();

    private readonly Dictionary<IStatContainer, List<ModifierHandle>> _activeBindings = new();

    private void OnValidate()
    {
        if (!Application.isPlaying) return;

        RebuildAllBindings();
    }

    public void ApplyTo(IStatContainer target)
    {
        if (target == null) return;

        RemoveFrom(target);

        var handles = new List<ModifierHandle>();
        foreach (var mod in Modifiers)
        {
            handles.Add(target.AddModifier(mod));
        }

        _activeBindings[target] = handles;
    }

    public void RemoveFrom(IStatContainer target)
    {
        if (target == null) return;

        if (_activeBindings.TryGetValue(target, out var handles))
        {
            foreach (var handle in handles)
            {
                target.RemoveModifier(handle);
            }
            _activeBindings.Remove(target);
        }
    }

    private void RebuildAllBindings()
    {
        var targets = new List<IStatContainer>(_activeBindings.Keys);

        foreach (var target in targets)
        {
            if (target == null || (target is UnityEngine.Object obj && obj == null))
            {
                _activeBindings.Remove(target);
                continue;
            }

            if (_activeBindings.TryGetValue(target, out var handles))
            {
                foreach (var handle in handles)
                {
                    target.RemoveModifier(handle);
                }
                handles.Clear();
            }
            else
            {
                handles = new List<ModifierHandle>();
                _activeBindings[target] = handles;
            }

            foreach (var mod in Modifiers)
            {
                handles.Add(target.AddModifier(mod));
            }
        }
    }

    private void OnDisable()
    {
        _activeBindings.Clear();
    }
}