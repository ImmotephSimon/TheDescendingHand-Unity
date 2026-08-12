using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAttackDef", menuName = "Enemies/Attacks/Melee Attack")]
public class MeleeAttackDefinition : EnemyAttackDefinition
{
    public override IEnemyAttack Create(Enemy owner)
    {
        var attack = owner.gameObject.AddComponent<MeleeAttack>();
        attack.Initialize(this);
        return attack;
    }
}