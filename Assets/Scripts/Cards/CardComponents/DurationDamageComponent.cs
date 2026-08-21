using System.Collections.Generic;
using UnityEngine;

public class DurationDamageComponent : CardComponent
{
    private ICalculator _calc;
    private readonly float _effectiveness;
    private readonly float _duration;
    private readonly int _maxStacks;

    private Dictionary<GameTag, float> _damage;
    private GameTag _damageConversion;

    public DurationDamageComponent(
        GameTag damageConversion,
        float effectiveness,
        float duration,
        int maxStacks)
    {
        _damageConversion = damageConversion;
        _effectiveness = effectiveness;
        _duration = duration;
        _maxStacks = maxStacks;
    }

    public override void Initialize(Card card, IEntity owner)
    {
        base.Initialize(card, owner);

        _calc = Owner.Transform.GetComponent<ICalculator>();
        Debug.Assert(_calc != null, $"Failed to find calculator.");
    }

    public override void OnHit(HitInfo info)
    {
        if (info.Target is IDamageable damageable)
        {
            var degenInfo = new DegenInfo(
                Card.Id,
                _damage,
                info.Source,
                info.Position,
                _duration,
                _maxStacks);

            damageable.ApplyDegen(degenInfo);
        }
    }

    public void ApplyDegen(IEntity target)
    {
        if (target is IDamageable damageable)
        {
            var degenInfo = new DegenInfo(
                Card.Id,
                _damage,
                Owner,
                target.Transform.position,
                _duration,
                _maxStacks);
            damageable.ApplyDegen(degenInfo);
        }
    }

    public void StopDegen(IEntity target)
    {
        if (target is IDamageable damageable)
        {
            damageable.RemoveDegen(Card.Id); 
        }
    }

    protected override void OnActivate()
    {
        _damage = _calc.CalculateDamage(Card.Tags, _effectiveness, _damageConversion);
    }
}