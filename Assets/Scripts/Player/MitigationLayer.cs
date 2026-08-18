using System.Collections.Generic;
using UnityEngine;

public class MitigationLayer : MonoBehaviour
{
    private IHealth _healthHandler;
    private IStatContainer _stats; // Assuming stats live on entity

    private void Awake()
    {
        _healthHandler = GetComponent<IHealth>();
        _stats = GetComponent<IStatContainer>();

        Debug.Assert(_healthHandler != null, $"Missing health handler");
        Debug.Assert(_stats != null, $"Missing stats");
    }


    public float CalculateMitigation(DamageInfo info)
    {
        if (info.DamageMap == null) return 0f;

        float maxLife = _healthHandler.MaxHealth;
        float totalDamage = 0f;

        foreach (var (damageType, rawAmount) in info.DamageMap)
        {
            float mitigationRating = _stats != null
                ? _stats.GetStat(GameTags.ModDefenseMitigation, new TagContainer(damageType))
                : 0f;

            // Rating == MaxLife yields 0.5 (50% damage taken)
            float damageMultiplier = maxLife / (maxLife + Mathf.Max(0f, mitigationRating));

            totalDamage += rawAmount * damageMultiplier;
        }

        return totalDamage;
    }

    public Dictionary<GameTag, float> CalculateMitigation(Dictionary<GameTag, float> damageMap)
    {
        var mitigated = new Dictionary<GameTag, float>(damageMap.Count);
        float maxLife = _healthHandler.MaxHealth;

        foreach (var (type, rawAmount) in damageMap)
        {
            float mitigation = _stats != null
                ? _stats.GetStat(GameTags.ModDefenseMitigation, new TagContainer(type))
                : 0f;

            mitigated[type] = rawAmount * (maxLife / (maxLife + Mathf.Max(0f, mitigation)));
        }

        return mitigated;
    }

    public float CalculateSingleMitigation(float rawAmount, GameTag damageType)
    {
        float maxLife = _healthHandler.MaxHealth;
        float mitigationRating = _stats != null
            ? _stats.GetStat(GameTags.ModDefenseMitigation, new TagContainer(damageType))
            : 0f;

        float damageMultiplier = maxLife / (maxLife + Mathf.Max(0f, mitigationRating));
        return rawAmount * damageMultiplier;
    }

}