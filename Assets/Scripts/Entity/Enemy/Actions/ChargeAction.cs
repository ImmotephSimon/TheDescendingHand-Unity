using UnityEngine;

public class ChargeAttack : EnemyAttack
{
    public override float Range => 10f;

    public override float CooldownDuration => 3f;

    public override void Execute(Transform target)
    {
        Debug.Log($"Charging towards {target.name}");
    }
}