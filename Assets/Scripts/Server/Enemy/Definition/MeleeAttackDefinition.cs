using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAttackDef", menuName = "Enemies/Attacks/Melee Attack")]
public class MeleeAttackDefinition : EnemyAttackDefinition
{
    public override IEnemyAttack Create(ServerEnemy owner)
    {
        var attack = owner.gameObject.AddComponent<MeleeAttack>();
        attack.Initialize(this, owner.HitBoxSampleData[animation], owner.transform);
        return attack;
    }
}