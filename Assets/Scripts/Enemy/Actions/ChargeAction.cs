using UnityEngine;

public class ChargeAttack : MonoBehaviour, IAttackAbility
{
    private AnimationClip animationClip;

    public float Range => 10f;
    public float CooldownDuration => 3f;

    public AnimationClip AttackAnimation => animationClip;

    public bool CanHit(Transform target)
    {
        return Vector3.Distance(transform.position, target.position) <= Range;
    }

    public void Execute(Transform target)
    {
        Debug.Log($"Charging towards {target.name}");
    }
}