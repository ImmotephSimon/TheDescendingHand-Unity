using UnityEngine;

public class MitigationLayer : MonoBehaviour
{
    private IHealth _healthHandler;
    private IAilmentHandler _ailmentHandler;
    private IStatContainer _statsHandler; // Assuming stats live on entity

    private void Awake()
    {
        _healthHandler = GetComponent<IHealth>();
        _ailmentHandler = GetComponent<IAilmentHandler>();
        _statsHandler = GetComponent<IStatContainer>();

        if (_healthHandler == null) Debug.LogError("Missing health handler");
        if (_ailmentHandler == null) Debug.LogError("Missing ailment handler");
    }

    public void TakeDamage(DamageInfo info)
    {
        float mitigatedDamage = CalculateMitigation(info);

        // Pass calculated final damage or update info if it's a class/ref
        _ailmentHandler.ApplyAilments(info, mitigatedDamage);
        _healthHandler.AdjustHealth(-mitigatedDamage, info.Source);
    }

    private float CalculateMitigation(DamageInfo info)
    {
        if (info.DamageMap == null) return 0f;

        float maxLife = _healthHandler.MaxHealth;
        float totalDamage = 0f;

        foreach (var (damageType, rawAmount) in info.DamageMap)
        {
            float mitigationRating = _statsHandler != null
                ? _statsHandler.GetStat(GameTags.ModDefenseMitigation, new TagContainer(damageType))
                : 0f;

            // Rating == MaxLife yields 0.5 (50% damage taken)
            float damageMultiplier = maxLife / (maxLife + Mathf.Max(0f, mitigationRating));

            totalDamage += rawAmount * damageMultiplier;
        }

        return totalDamage;
    }
}