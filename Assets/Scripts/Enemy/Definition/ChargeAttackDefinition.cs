// 1. DEFINITION (Data asset for Charge Attack)
using UnityEngine;

[CreateAssetMenu(fileName = "ChargeAttackDef", menuName = "Enemies/Attacks/Charge Attack")]
public class ChargeAttackDefinition : EnemyAttackDefinition
{
    public override IEnemyAttack Create(Enemy owner)
    {
        var attack = owner.gameObject.AddComponent<ChargeAttack>();
        attack.Initialize(this);
        return attack;
    }
}