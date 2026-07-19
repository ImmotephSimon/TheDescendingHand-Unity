using UnityEngine;

public class MeleeAttack : MonoBehaviour, IAttackAbility
{
    [SerializeField] private AnimationClip animationClip;
    [SerializeField] private float range = 2f;
    [SerializeField] private float attackAngle = 45f;
    public float Range => range;

    public float CooldownDuration => 2f;

    public AnimationClip AttackAnimation => animationClip;

    public bool CanHit(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.magnitude > range)
            return false;

        float angle = Vector3.Angle(transform.forward, direction);

        return angle <= attackAngle;
    }

    public void Execute(Transform target)
    {
        Debug.Log($"Melee attacking {target.name}");
    }
}