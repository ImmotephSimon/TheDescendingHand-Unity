using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class DirectDamageComponent : CardComponent
{
    private Dictionary<GameTag, float> _damage = new();
    private readonly GameTag _damageConversion;
    private readonly bool _triggerOnHit;
    private readonly float _effectiveness;
    private ICalculator _calc;

    public DirectDamageComponent(float effectiveness, GameTag damageConversion, bool triggerOnHit = true)
    {
        _effectiveness = effectiveness;
        _damageConversion = damageConversion;
        _triggerOnHit = triggerOnHit;
    }

    public override void Initialize(Card card, IEntity owner)
    {
        base.Initialize(card, owner);

        _calc = Card.Owner.Transform.GetComponent<ICalculator>();
        Debug.Assert(_calc != null, $"Failed to find calculator.");

        if (_triggerOnHit)
            Card.OnHit += HandleHit;
    }

    private void HandleHit(HitInfo info)
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

    public void TriggerDamage(HitInfo hit, float scalar = 1f)
    {
        var damage = _damage.ToDictionary(x => x.Key, x => x.Value * scalar);

        if (hit.Target is IDamageable damageable)
            damageable.TakeDamage(new DamageInfo(damage, hit.Source, hit.Position));
    }


    protected override void OnActivate()
    {
        _damage = _calc.CalculateDamage(Card.Tags, _effectiveness, _damageConversion);
    }

}