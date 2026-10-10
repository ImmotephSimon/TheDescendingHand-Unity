using UnityEngine;

public class ChargeAttack : EnemyAttack
{
    private float rangeTolerance;

    public override void Initialize(EnemyAttackDefinition def)
    {
        base.Initialize(def);
        rangeTolerance = ((ChargeAttackDefinition)def).rangeTolerance;
    }

    public override bool CanHit(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        float d = dir.magnitude;
        return d <= Range && d >= Range - rangeTolerance;
    }
}