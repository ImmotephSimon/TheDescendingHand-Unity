using System;
using UnityEngine;

public class AilmentComponent : MonoBehaviour, IAilmentHandler
{
    [SerializeField] private float poiseMultiplier = 2;
    [SerializeField] private float stunThreshold = 0.2f;

    private IHealth _health;
    private IStunnable _stunnable;
    private float _poise;
    

    private float PoiseThreshold => _health.MaxHealth * stunThreshold * poiseMultiplier;

    private void Awake()
    {
        _health = GetComponent<IHealth>();
        _stunnable = GetComponent<IStunnable>();
    }

    public void ApplyAilments(DamageInfo info)
    {
        CalcStun(info);
    }

    private void CalcStun(DamageInfo info)
    {
        _poise += info.Amount;

        if (info.Amount >= _health.MaxHealth * stunThreshold)
        {
            ApplyStun(info.Amount);
        }
        else
        {
            if (_poise >= PoiseThreshold)
            {
                ApplyStun(info.Amount);
            }
        }
    }

    private void ApplyStun(float damage)
    {
        
        float duration = CalculateStunDuration(damage);
        _stunnable.ApplyStun(Mathf.Max(duration, 1));
        _poise = 0;
    }

    private float CalculateStunDuration(float damage)
    {
        return 1f;
    }
}