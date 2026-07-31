using System;
using UnityEngine;

public class AilmentComponent : MonoBehaviour, IAilmentHandler
{
    [SerializeField] private float poiseMultiplier = 2;
    [SerializeField] private float stunThreshold = 0.2f;
    [SerializeField] private float stunImmunityDuration = 2f;

    private IHealth _health;
    private IStunnable _stunnable;
    private float _poise;
    private float _stunImmunityTimer;

    private float PoiseThreshold => _health.MaxHealth * stunThreshold * poiseMultiplier;

    private void Awake()
    {
        _health = GetComponent<IHealth>();
        _stunnable = GetComponent<IStunnable>();
    }

    private void Update()
    {
        if (_stunImmunityTimer > 0)
            _stunImmunityTimer -= Time.deltaTime;
    }

    public void ApplyAilments(DamageInfo info)
    {
        CalcStun(info);
    }

    private void CalcStun(DamageInfo info)
    {
        _poise += info.Amount;

        if (_stunImmunityTimer > 0)
            return;

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
        _poise = 0;
        _stunImmunityTimer = stunImmunityDuration;

        float duration = CalculateStunDuration(damage);
        _stunnable.ApplyStun(Mathf.Max(duration, 1));
    }

    private float CalculateStunDuration(float damage)
    {
        return 1f;
    }
}