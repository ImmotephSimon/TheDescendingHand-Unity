using Cards.CardComponents;
using UnityEngine;

public class DirectDamageComponent : CardComponent
{
    private readonly Vector2 _damageRange;
    private readonly float _effectiveness;

    public DirectDamageComponent(Vector2 damageRange, float effectiveness)
    {
        _damageRange = damageRange;
        _effectiveness = effectiveness;
    }

    public override void OnHit(HitInfo info)
    {
        float baseDamage = Random.Range(_damageRange.x, _damageRange.y);
        float damage = baseDamage * _effectiveness;

        if (info.Target.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(Mathf.RoundToInt(damage));
        }
    }
}