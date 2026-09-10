using System.Collections.Generic;
using UnityEngine;

public class DamageMitigationStep : IDamagePipelineStep
{
    public bool RunStep(IEntity owner, DamageInfo info)
    {
        var health = owner.Transform.GetComponent<IHealth>();
        var stats = owner.Transform.GetComponent<IStatContainer>();

        Debug.Assert(health != null, "Missing health handler");
        Debug.Assert(stats != null, "Missing stats");

        float maxLife = health.MaxHealth;
        var keys = new List<GameTag>(info.DamageMap.Keys);

        foreach (var type in keys)
        {
            float mitigation = stats.GetStat(GameTags.ModDefenseMitigation, new TagContainer(type));
            info.DamageMap[type] *= maxLife / (maxLife + Mathf.Max(0f, mitigation));
        }

        return true;
    }
}