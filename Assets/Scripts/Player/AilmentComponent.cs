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

    public void ApplyAilments(DamageInfo info, float mitigatedDamage)
    {
        CalcStun(mitigatedDamage);
        ApplyDamageAilments(info, mitigatedDamage);
    }

    private void CalcStun(float damage)
    {
        _poise += damage;

        if (_stunImmunityTimer > 0)
            return;

        if (damage >= _health.MaxHealth * stunThreshold)
        {
            ApplyStun(damage);
        }
        else
        {
            if (_poise >= PoiseThreshold)
            {
                ApplyStun(damage);
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

    private void ApplyDamageAilments(DamageInfo info, float mitigatedDamage)
    {
        if (info.Tags.HasTag(GameTags.ModSpecialColdDamageCanIgnite))
            ApplyIgnite(info, mitigatedDamage);

        if (info.Tags.HasTag(GameTags.ModSpecialPoisonDamageCanFreeze))
            ApplyFreeze(info, mitigatedDamage);
    }

    private void ApplyFreeze(DamageInfo info, float mitigatedDamage)
    {
        throw new NotImplementedException();
    }

    private void ApplyIgnite(DamageInfo info, float mitigatedDamage)
    {
        throw new NotImplementedException();
    }
}