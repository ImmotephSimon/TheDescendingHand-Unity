using System.Collections.Generic;
using UnityEngine;

public class DamageImmunityStep : IDamagePipelineStep
{
    private readonly Dictionary<GameTag, float> _lockoutExpirations = new();
    private readonly List<GameTag> _keysToRemove = new();

    public bool RunStep(IEntity owner, DamageInfo info)
    {
        if (info.DamageMap == null || info.DamageMap.Count == 0)
            return false;

        // Stat check: 0 or non-existent means feature is inactive on this owner
        float lockoutValue = info.Source.Stats.GetStat(GameTags.ModSpecialDamageTypeLockout);
        if (lockoutValue <= 0f)
            return true;

        float currentTime = Time.time;
        _keysToRemove.Clear();

        foreach (var pair in info.DamageMap)
        {
            GameTag damageType = pair.Key;

            // 1. Check if damageType is currently locked out
            if (_lockoutExpirations.TryGetValue(damageType, out float expireTime) && currentTime < expireTime)
            {
                _keysToRemove.Add(damageType);
            }
            else
            {
                // 2. Active hit updates/refreshes lockout timer using the stat duration
                _lockoutExpirations[damageType] = currentTime + lockoutValue;
            }
        }

        // Strip locked out types
        for (int i = 0; i < _keysToRemove.Count; i++)
        {
            info.DamageMap.Remove(_keysToRemove[i]);
        }

        return info.DamageMap.Count > 0;
    }
}