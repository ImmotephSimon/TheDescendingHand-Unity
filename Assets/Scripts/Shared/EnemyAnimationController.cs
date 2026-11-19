using System;
using System.Collections;
using UnityEngine;

public class EnemyAnimationController : MonoBehaviour, IEnemyAnimationHandler
{
    private const float TransitionDuration = 0.1f;

    private Animator _animator;
    private CharacterAnimationState _currentState;
    private Coroutine _attackCoroutine;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        Debug.Assert(_animator != null, $"Missing animator on {transform.name}");
        Debug.Log($"Animator enabled: {_animator.enabled}, active: {_animator.gameObject.activeInHierarchy}, controller: {_animator.runtimeAnimatorController}");
    }

    private EnemyStatsNetwork _network;

    private void OnEnable()
    {
        _network = GetComponentInParent<EnemyStatsNetwork>();
        if (_network == null) return;

        _network.AnimationStateChanged += SetAnimationState;
        _network.AttackAnimationPlayed += PlayAttackAnimation;
    }

    private void OnDisable()
    {
        if (_network == null) return;

        _network.AnimationStateChanged -= SetAnimationState;
        _network.AttackAnimationPlayed -= PlayAttackAnimation;
    }

    public void HitboxEnable() { }
    public void HitboxDisable() { }

    public void SetSpeed(float speed)
    {
        _animator.SetFloat("Speed", speed);
    }

    public void SetAnimationState(CharacterAnimationState state)
    {
        PlayState(state);
    }

    public void PlayAttackAnimation(AttackAnimation attackAnimation, float duration)
    {
        StopCurrentAnimation();
        _currentState = CharacterAnimationState.Unset;

        var state = attackAnimation.StateName();

        _animator.CrossFade(state, TransitionDuration);
        _attackCoroutine = StartCoroutine(FinishAttack(state));
    }


    public void StopCurrentAnimation()
    {
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }
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
            CharacterAnimationState.Immobilized => "Stunned",
            CharacterAnimationState.Dead => "Dead",
            _ => throw new ArgumentOutOfRangeException(nameof(state))
        };

        _animator.CrossFade(stateName, transitionDuration);
    }

    private IEnumerator FinishAttack(string state)
    {
        yield return new WaitUntil(() =>
            _animator.GetCurrentAnimatorStateInfo(0).IsName(state));

        var duration = _animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(duration);

        SetAnimationState(CharacterAnimationState.Locomotion);
        _attackCoroutine = null;
    }

    private void OnDestroy()
    {
        StopCurrentAnimation();
    }

    public Action PlayAnimation(AnimationClip clip, float duration)
    {
        return null;
    }
}