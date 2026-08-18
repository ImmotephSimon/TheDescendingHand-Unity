using UnityEngine;

[CreateAssetMenu(fileName = "EnemyCardDef", menuName = "Enemies/Attacks/Card Attack")]
public class EnemyCardAttackDefinition : EnemyAttackDefinition
{
    [SerializeField] private CardDefinition cardDefinition;

    public CardDefinition CardDefinition => cardDefinition;

    public override IEnemyAttack Create(Enemy owner)
    {
        var attack = owner.gameObject.AddComponent<EnemyCardAttack>();
        attack.Initialize(this);
        return attack;
    }
}