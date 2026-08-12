using UnityEngine;

public class ChargeAttack : EnemyAttack
{
    public override void Execute(Transform target)
    {
        Debug.Log($"Charging towards {target.name}");
    }
}