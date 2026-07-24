using UnityEngine;

public class MitigationLayer : MonoBehaviour, IDamageable
{
    private HealthComponent _healthHandler;

    private void Awake()
    {
        _healthHandler = GetComponent<HealthComponent>();
    }

    public void TakeDamage(DamageInfo info)
    {
        float finalDamage = CalculateMitigation(info);

        _healthHandler.AdjustHealth(finalDamage);
    }

    private float CalculateMitigation(DamageInfo info)
    {
        return info.Amount;
    }
}