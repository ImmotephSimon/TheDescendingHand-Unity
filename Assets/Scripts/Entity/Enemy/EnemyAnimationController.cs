using System;
using System.Collections;
using UnityEngine;



public class EnemyAnimationController : MonoBehaviour, IAnimationHandler
{
    [SerializeField] AnimationClip fastAttack;
    [SerializeField] AnimationClip slowAttack;
    [SerializeField] AnimationClip chargeAttack;

    private const float _transitionDuration = 0.1f;
    private Animator animator;
    private CharacterAnimationState currentState;
    private AnimatorOverrideController _overrideController;
    private Action attackFinished;
    private int _attackHash;
    private Coroutine _currentHandle;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int LocomotionHash = Animator.StringToHash("EnemyLocomotion");
    private static readonly int StunHash = Animator.StringToHash("Stunned");
    private static readonly int DeadHash = Animator.StringToHash("Dead");
    private static readonly int AttackingHash = Animator.StringToHash("Attacking");

   
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
        animator.SetFloat(SpeedHash, speed);
    }

    public void PlayState(CharacterAnimationState state, float transitionDuration = _transitionDuration)
    {
        if (currentState == state)
            return;

        if (state == CharacterAnimationState.Unset)
            return;

        currentState = state;
        Debug.Log(
            $"ANIM {gameObject.name} id={gameObject.GetInstanceID()} -> {state} frame={Time.frameCount}\n" +
            $"caller:\n{Environment.StackTrace}"
        );
        switch (state)
        {
            case CharacterAnimationState.Locomotion:
                animator.CrossFade(LocomotionHash, transitionDuration);
                break;

            case CharacterAnimationState.Attack:
                animator.CrossFade(AttackingHash, transitionDuration);
                break;

            case CharacterAnimationState.Stun:
                animator.CrossFade(StunHash, transitionDuration);
                break;

            case CharacterAnimationState.Dead:
                animator.CrossFade(DeadHash, transitionDuration);

                if (_currentHandle != null)
                {
                    StopCoroutine(_currentHandle);
                    _currentHandle = null;
                }
                break;
        }
    }

    public void PlayAnimation(AttackAnimation attackAnimation, Action onFinished)
    {
        attackFinished = onFinished;

        if (currentState != CharacterAnimationState.Attack)
            PlayState(CharacterAnimationState.Attack, _transitionDuration);
        else animator.Play(AttackingHash, 0, 0f);

        AnimationClip clip = attackAnimation switch
        {
            AttackAnimation.MeleeFast => fastAttack,
            AttackAnimation.MeleeSlow => slowAttack,
            AttackAnimation.MeleeCharge => chargeAttack,
            _ => fastAttack
        };

        _currentHandle = StartCoroutine(WaitForAttackFinished(clip.length, onFinished));
    }

    private IEnumerator WaitForAttackFinished(float duration, Action onFinished)
    {
        yield return new WaitForSeconds(duration);
        onFinished?.Invoke();
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