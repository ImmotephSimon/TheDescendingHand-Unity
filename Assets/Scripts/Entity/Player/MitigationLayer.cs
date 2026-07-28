using UnityEngine;

public class MitigationLayer : MonoBehaviour
{
    private IHealth _healthHandler;
    private IAilmentHandler _ailmentHandler;

    private void Awake()
    {
        _healthHandler = GetComponent<IHealth>();
        _ailmentHandler = GetComponent<IAilmentHandler>();
        if (_healthHandler == null) Debug.LogError("Missing health handler");
        if (_ailmentHandler == null) Debug.LogError("Missing ailment handler");
    }

    public void TakeDamage(DamageInfo info)
    {
        float finalDamage = CalculateMitigation(info);
        info.Amount = finalDamage;

        _ailmentHandler.ApplyAilments(info);

        _healthHandler.AdjustHealth(-finalDamage, info.Source);
    }

    private float CalculateMitigation(DamageInfo info)
    {
        return info.Amount;
    }
}