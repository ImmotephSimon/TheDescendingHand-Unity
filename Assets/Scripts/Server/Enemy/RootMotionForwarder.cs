using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RootMotionForwarder : MonoBehaviour
{
    private Animator animator;
    private Transform target;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        target = transform.parent.parent; // CagedBrute -> VisualAnchor -> Enemy
    }

    private void OnAnimatorMove()
    {
        target.position += animator.deltaPosition;
        target.rotation *= animator.deltaRotation;
    }
}