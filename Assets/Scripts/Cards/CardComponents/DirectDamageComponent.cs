using Cards.CardComponents;
using System.Collections.Generic;
using UnityEngine;
using static DirectDamageComponent;

public class DamagePayload
{
    public readonly List<DamagePortion> Portions = new();

    public void AddPortion(DamagePortion portion)
    {
        Portions.Add(portion);
    }
}

public class DirectDamageComponent : CardComponent
{
    public readonly struct DamagePortion
    {
        public readonly TagRestriction DamageType;
        public readonly float Amount;

        public DamagePortion(TagRestriction damageType, float amount)
        {
            DamageType = damageType;
            Amount = amount;
        }
    }

    private readonly Dictionary<Restriction, float> _damage = new();
    private readonly TagRestriction _damageConversion;
    private readonly float _effectiveness;

    public DirectDamageComponent(float effectiveness, TagRestriction damageConversion)
    {
        _effectiveness = effectiveness;
        Debug.Log($"Damage is hardcoded.");
        _damageConversion = damageConversion;
        _damage.Add(_damageConversion, 2);

    }


    public override void OnHit(HitInfo info)
    {
        float amount = 0f;
        foreach (KeyValuePair<Restriction, float> pair in _damage)
        {
            amount += pair.Value;
        }

        DamageInfo damageInfo = new(
            amount,
            info.Source,
            info.Position
        );
        if (info.Target is IDamageable damageable)
        {
            damageable.TakeDamage(damageInfo);
        }
    }


    protected override void OnActivate()
    {
    }

    protected override void OnBegin()
    {
    }

    protected override void OnCancel()
    {
    }
}