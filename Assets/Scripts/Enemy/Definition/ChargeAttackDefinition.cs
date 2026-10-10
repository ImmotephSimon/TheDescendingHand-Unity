using UnityEngine;

[CreateAssetMenu(fileName = "ChargeAttackDef", menuName = "Enemies/Attacks/Charge Attack")]
public class ChargeAttackDefinition : EnemyAttackDefinition
{

    public float rangeTolerance = 1f;

    public override IEnemyAttack Create(Enemy owner)
    {
        var attack = owner.gameObject.AddComponent<ChargeAttack>();
        attack.Initialize(this);
        return attack;
    }
}