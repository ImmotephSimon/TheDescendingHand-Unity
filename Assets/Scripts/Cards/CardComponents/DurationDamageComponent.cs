using System.Collections.Generic;
using System.Diagnostics;

namespace Cards.CardComponents
{
    public class DurationDamageComponent : CardComponent
    {
        private readonly ICalculator _calc;
        private readonly float _effectiveness;
        private readonly float _duration;
        private readonly float _tickInterval;
        private readonly int _maxStacks;

        private Dictionary<GameTag, float> _damage;
        private TagRestriction _damageConversion;

        public DurationDamageComponent(
            TagRestriction damageConversion,
            float effectiveness,
            float duration,
            float tickInterval,
            int maxStacks)
        {
            _damageConversion = damageConversion;
            _effectiveness = effectiveness;
            _duration = duration;
            _tickInterval = tickInterval;
            _maxStacks = maxStacks;

            _calc = Owner.Transform.GetComponent<ICalculator>();
            Debug.Assert(_calc != null, $"Failed to find calculator.");
        }

        public override void OnHit(HitInfo info)
        {
            if (info.Target is IDamageable damageable)
            {
                var degenInfo = new DegenInfo(
                    _damage,
                    info.Source,
                    info.Position,
                    _duration,
                    _tickInterval,
                    _maxStacks);

                damageable.ApplyDegen(degenInfo);
            }
        }

        protected override void OnBegin()
        {
        }

        protected override void OnActivate()
        {
            _damage = _calc.CalculateDamage(Card.Tags, _effectiveness, _damageConversion);
        }
    }
}