using UnityEngine;

namespace Cards.CardComponents
{
    public class DurationDamageComponent : CardComponent
    {
        private readonly float _duration;
        private readonly float _tickInterval;
        private readonly int _damagePerTick;

        private float _elapsed;
        private float _tickTimer;

        public DurationDamageComponent(
            float duration,
            float tickInterval,
            int damagePerTick)
        {
            _duration = duration;
            _tickInterval = tickInterval;
            _damagePerTick = damagePerTick;
        }

        protected virtual void Complete()
        {
            OnComplete();
        }
        protected virtual void OnComplete() { }

        public override void Tick(float deltaTime)
        {
            _elapsed += deltaTime;
            _tickTimer += deltaTime;

            if (_elapsed >= _duration)
            {
                Complete();
                return;
            }

            if (_tickTimer >= _tickInterval)
            {
                _tickTimer -= _tickInterval;
                ApplyTickDamage();
            }
        }

        private void ApplyTickDamage()
        {
            // Needs target from whoever applied this effect
        }

        protected override void OnBegin()
        {
        }

        protected override void OnActivate()
        {
        }

        protected override void OnCancel()
        {
        }
    }
}