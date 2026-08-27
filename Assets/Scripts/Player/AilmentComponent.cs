using System;
using UnityEngine;

public class AilmentComponent : MonoBehaviour, IAilmentHandler
{
    private float poiseMultiplier = 2;
    private float stunImmunityDuration = 2f;

    private float StunThreshold => _stats.GetStat(GameTags.ModStatStunThreshold);
    private float StunDuration => _stats.GetStat(GameTags.ModStatStunDuration);

    private IStatContainer _stats;
    private IStunnable _stunnable;
    private float _poise;
    private float _stunImmunityTimer;

    private void Awake()
    {
        _stats = GetComponent<IStatContainer>();
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

        float maxHealth = _stats.GetStat(GameTags.ModStatHealth);


        if (_stunImmunityTimer > 0)
            return;

        if (damage >= maxHealth * StunThreshold)
        {
            ApplyStun(damage);
        }
        else
        {
            if (_poise >= maxHealth * StunThreshold * poiseMultiplier)
            {
                ApplyStun(damage);
            }
        }
    }

    private void ApplyStun(float damage)
    {
        _poise = 0;
        _stunImmunityTimer = stunImmunityDuration;

        _stunnable.ApplyStun(StunDuration);
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