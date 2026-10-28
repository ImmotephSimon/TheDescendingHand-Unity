using System;
using UnityEngine;

public interface IDamageable
{
    void ApplyDegen(DegenInfo degenInfo);
    void RemoveDegen(Guid id);
    void TakeDamage(DamageInfo info);
}