using UnityEngine;

public interface IDamageable
{
    void ApplyDegen(DegenInfo degenInfo);
    void TakeDamage(DamageInfo info);
}