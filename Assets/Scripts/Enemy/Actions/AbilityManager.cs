using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour, IAbilityManager
{
    private readonly Dictionary<IAttackAbility, float> _cooldowns = new();
    private readonly List<IAttackAbility> _keys = new();

    public bool Ready(IAttackAbility ability)
    {
        return !_cooldowns.TryGetValue(ability, out var time) || time <= 0;
    }

    public void StartCooldown(IAttackAbility ability)
    {
        _cooldowns[ability] = ability.CooldownDuration;
    }

    private void Update()
    {
        if (_cooldowns.Count == 0) return;

        float dt = Time.deltaTime;

        _keys.Clear();
        foreach (var key in _cooldowns.Keys)
        {
            _keys.Add(key);
        }

        for (int i = 0; i < _keys.Count; i++)
        {
            var ability = _keys[i];
            float remaining = _cooldowns[ability] - dt;

            if (remaining <= 0)
                _cooldowns.Remove(ability);
            else
                _cooldowns[ability] = remaining;
        }
    }
}