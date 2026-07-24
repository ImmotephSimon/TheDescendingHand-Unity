using System;
using System.Collections;
using UnityEngine;



public class EnemyAnimationController : MonoBehaviour, IAnimationHandler
{
    [SerializeField] private AnimationClip fastAttack;
    [SerializeField] private AnimationClip slowAttack;
    [SerializeField] private AnimationClip chargeAttack;

    private Animator animator;
    private CharacterAnimationState currentState;
    private bool isLocked;
    private AnimatorOverrideController _overrideController;
    private Action attackFinished;
    public event Action AttackHit;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int StunHash = Animator.StringToHash("Stun");
    private static readonly int DeathHash = Animator.StringToHash("Death");

    

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError($"[{name}] EnemyAnimationController failed to find an Animator component in children!", this);
            return;
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError($"[{name}] The Animator on child '{animator.gameObject.name}' is missing a base Runtime Animator Controller asset in the inspector!", this);
            return;
        }
        _overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = _overrideController;

    }

    public void SetSpeed(float speed)
    {
        if (isLocked) return;
        animator.SetFloat(SpeedHash, speed);
    }



    public void PlayState(CharacterAnimationState state, float transitionDuration = 0.1f)
    {
        if (isLocked) return;
        if (currentState == state) return;
        if (state == CharacterAnimationState.Unset) return;

        currentState = state;

        switch (state)
        {
            case CharacterAnimationState.Locomotion:
                animator.CrossFade(LocomotionHash, transitionDuration);
                break;

            case CharacterAnimationState.Attack:
                Debug.Log($"Playing animator state {state}");
                animator.CrossFade(AttackHash, transitionDuration);
                break;

            case CharacterAnimationState.Stun:
                Debug.Log($"Playing animator state {state}");
                animator.CrossFade(StunHash, transitionDuration);
                break;

            case CharacterAnimationState.Dead:
                Debug.Log($"Playing animator state {state}");
                animator.CrossFade(DeathHash, transitionDuration);
                isLocked = true;
                break;
        }
    }

    // Called via Unity Animation Event when the swing connects
    public void OnAttackHit()
    {
        AttackHit?.Invoke();

        Debug.Log($"Attack hit event invoked");
    }

    public void PlayAnimation(AnimationClip clip, Action onFinished)
    {
        attackFinished = onFinished;
        _overrideController["Attack"] = clip;
        animator.CrossFade("Attack", 0.1f);
    }

    public void OnAttackFinished()
    {
        attackFinished?.Invoke();
        attackFinished = null;
    }

    public void PlayAnimation(AttackAnimation attackAnimation, Action onFinished)
    {
        attackFinished = onFinished;

        AnimationClip clip = attackAnimation switch
        {
            AttackAnimation.MeleeFast => fastAttack,
            AttackAnimation.MeleeSlow => slowAttack,
            AttackAnimation.MeleeCharge => chargeAttack,
            _ => fastAttack
        };

        _overrideController["Attack"] = clip;
        animator.CrossFade(AttackHash, 0.1f);
    }
    public void SetAnimationState(CharacterAnimationState state)
    {
        PlayState(state);
    }


    public void PlayAnimation(CardCastAnimation animation)
    {
        throw new NotImplementedException();
    }
}