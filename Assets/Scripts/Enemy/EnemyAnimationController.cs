using System;
using System.Collections;
using UnityEngine;

public class EnemyAnimationController : MonoBehaviour, IAnimationHandler
{
    [SerializeField] private AnimationClip fastAttack;
    [SerializeField] private AnimationClip slowAttack;
    [SerializeField] private AnimationClip chargeAttack;

    private const float _transitionDuration = 0.1f;
    private Animator animator;
    private CharacterAnimationState currentState;
    private AnimatorOverrideController _overrideController;
    private Action attackFinished;
    private Coroutine _currentHandle;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int LocomotionHash = Animator.StringToHash("EnemyLocomotion");
    private static readonly int StunHash = Animator.StringToHash("Stunned");
    private static readonly int DeadHash = Animator.StringToHash("Dead");
    private static readonly int AttackingHash = Animator.StringToHash("Attacking");

    private void Awake()
    {
        EnsureAnimatorSetup();
    }

    private void EnsureAnimatorSetup()
    {
        if (animator != null) return;

        animator = GetComponentInChildren<Animator>();
        if (animator == null) return;

        animator.applyRootMotion = false;
        _overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = _overrideController;
    }

    // --- IAnimationHandler Implementation ---

    public void SetSpeed(float speed)
    {
        EnsureAnimatorSetup();
        if (animator != null)
            animator.SetFloat(SpeedHash, speed);
    }

    public void SetAnimationState(CharacterAnimationState state)
    {
        PlayState(state);
    }

    public void PlayAnimation(AttackAnimation attackAnimation, Action onFinished)
    {
        StopCurrentAnimation();

        attackFinished = onFinished;

        if (currentState != CharacterAnimationState.Attack)
            PlayState(CharacterAnimationState.Attack, _transitionDuration);
        else
            animator.Play(AttackingHash, 0, 0f);

        AnimationClip clip = attackAnimation switch
        {
            AttackAnimation.MeleeFast => fastAttack,
            AttackAnimation.MeleeSlow => slowAttack,
            AttackAnimation.MeleeCharge => chargeAttack,
            _ => fastAttack
        };

        if (clip != null)
        {
            _currentHandle = StartCoroutine(WaitForAttackFinished(clip.length, onFinished));
        }
        else
        {
            onFinished?.Invoke();
        }
    }

    public void PlayAnimation(CardCastAnimation animation)
    {
        // Enemies don't cast card animations, but implement gracefully
        StopCurrentAnimation();
    }

    public void StopCurrentAnimation()
    {
        if (_currentHandle != null)
        {
            StopCoroutine(_currentHandle);
            _currentHandle = null;
        }

        attackFinished = null;
    }

    // --- State Logic ---

    public void PlayState(CharacterAnimationState state, float transitionDuration = _transitionDuration)
    {
        if (currentState == state || state == CharacterAnimationState.Unset)
            return;

        currentState = state;
        EnsureAnimatorSetup();

        if (animator == null) return;

        switch (state)
        {
            case CharacterAnimationState.Locomotion:
                animator.CrossFade(LocomotionHash, transitionDuration);
                break;

            case CharacterAnimationState.Attack:
                animator.CrossFade(AttackingHash, transitionDuration);
                break;

            case CharacterAnimationState.Stun:
                StopCurrentAnimation();
                animator.CrossFade(StunHash, transitionDuration);
                break;

            case CharacterAnimationState.Dead:
                StopCurrentAnimation();
                animator.CrossFade(DeadHash, transitionDuration);
                break;
        }
    }

    private IEnumerator WaitForAttackFinished(float duration, Action onFinished)
    {
        yield return new WaitForSeconds(duration);
        _currentHandle = null;
        onFinished?.Invoke();
    }

    private void OnDestroy()
    {
        StopCurrentAnimation();
    }
}