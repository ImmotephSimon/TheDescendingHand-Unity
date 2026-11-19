using System.Collections.Generic;
using UnityEngine;

public class ChargeAttack : EnemyAttack
{
    private float rangeTolerance;

    public override void Initialize(EnemyAttackDefinition def, List<HitBoxSample> hitboxSamples, Transform visualRoot)
    {
        base.Initialize(def, hitboxSamples, visualRoot);
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