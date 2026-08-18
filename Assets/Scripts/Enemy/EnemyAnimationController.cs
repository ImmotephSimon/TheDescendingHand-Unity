using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class EnemyAnimationController : MonoBehaviour, IAnimationHandler
{
    private const float TransitionDuration = 0.1f;

    private Animator _animator;
    private CharacterAnimationState _currentState;
    private Action _attackFinished;
    private Coroutine _attackCoroutine;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _animator.applyRootMotion = false;

    }



    public void SetSpeed(float speed)
    {
        _animator.SetFloat("Speed", speed);
    }

    public void SetAnimationState(CharacterAnimationState state)
    {
        PlayState(state);
    }

    public void PlayAnimation(AttackAnimation attackAnimation, Action onFinished)
    {
        StopCurrentAnimation();
        _currentState = CharacterAnimationState.Unset;

        var state = attackAnimation switch
        {
            AttackAnimation.MeleeFast => "FastAttack",
            AttackAnimation.MeleeSlow => "SlowAttack",
            AttackAnimation.Charge => "ChargeAttack",
            AttackAnimation.Special => "SpecialAttack",
            _ => throw new ArgumentOutOfRangeException(nameof(attackAnimation))
        };

        _attackFinished = onFinished;

        _animator.CrossFade(state, TransitionDuration);
        _attackCoroutine = StartCoroutine(FinishAttack(state, onFinished));
    }

    public void PlayAnimation(CardCastAnimation animation)
    {
        StopCurrentAnimation();
    }

    public void StopCurrentAnimation()
    {
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }

        _attackFinished = null;
    }

    public void PlayState(
        CharacterAnimationState state,
        float transitionDuration = TransitionDuration)
    {
        

        if (state == CharacterAnimationState.Unset || _currentState == state)
            return;

        _currentState = state;


        var stateName = state switch
        {
            CharacterAnimationState.Locomotion => "EnemyLocomotion",
            CharacterAnimationState.Stun => "Stunned",
            CharacterAnimationState.Dead => "Dead",
            _ => throw new ArgumentOutOfRangeException(nameof(state))
        };

        _animator.CrossFade(stateName, transitionDuration);
    }

    private IEnumerator FinishAttack(string state, Action onFinished)
    {
        yield return new WaitUntil(() =>
            _animator.GetCurrentAnimatorStateInfo(0).IsName(state));

        var duration = _animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(duration);

        SetAnimationState(CharacterAnimationState.Locomotion);
        _attackCoroutine = null;
        var finished = _attackFinished;
        _attackFinished = null;
        finished?.Invoke();
    }

    private void OnDestroy()
    {
        StopCurrentAnimation();
    }
}