using System.Collections.Generic;
using UnityEngine;


public class DirectDamageComponent : CardComponent
{
    private Dictionary<GameTag, float> _damage = new();
    private readonly TagRestriction _damageConversion;
    private readonly float _effectiveness;
    private ICalculator _calc;

    public override bool IsTicking => false;

    public DirectDamageComponent(float effectiveness, TagRestriction damageConversion)
    {
        _effectiveness = effectiveness;
        _damageConversion = damageConversion;
    }

    public override void Initialize(Card card, IEntity owner)
    {
        base.Initialize(card, owner);
        
        _calc = Card.Owner.Transform.GetComponent<ICalculator>();
        Debug.Assert(_calc != null, $"Failed to find calculator.");
    }


    public override void OnHit(HitInfo info)
    {
        DamageInfo damageInfo = new(
            _damage,
            info.Source,
            info.Position
        );

        if (info.Target is IDamageable damageable)
        {
            damageable.TakeDamage(damageInfo);
        }
    }

    public void ForceDamage(HitInfo hit, float scalar)
    {
        var damageDict = _calc.CalculateDamage(Card.Tags, _effectiveness * scalar, _damageConversion);

        if (hit.Target is IDamageable damageable)
        {
            damageable.TakeDamage(new DamageInfo(damageDict, hit.Source, hit.Position));
        }
    }


    protected override void OnActivate()
    {
        _damage = _calc.CalculateDamage(Card.Tags, _effectiveness, _damageConversion);
    }

    protected override void OnBegin()
    {
    }
}