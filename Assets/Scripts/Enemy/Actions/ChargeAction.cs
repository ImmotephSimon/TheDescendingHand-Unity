using UnityEngine;

public class ChargeAttack : EnemyAttack
{
    public override float CooldownDuration => 5f;

    public override void Execute(Transform target)
    {
        Debug.Log($"Charging towards {target.name}");
    }
}