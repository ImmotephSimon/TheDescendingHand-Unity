using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayerAnimationController : MonoBehaviour, IAnimationHandler
{
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip dodgeAnimation;

    private PlayableGraph _graph;
    private AnimationMixerPlayable _mixer;
    private Coroutine _routine;
    private const float TransitionDuration = 0.1f;
    private PlayerMovementController _playerMovement;

    private static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
    private static readonly int DeadHash = Animator.StringToHash("Dead");
    private static readonly int ImmobilizationHash = Animator.StringToHash("Immobilized");

    private readonly HashSet<GameTag> _activeStatuses = new();

    public Animator Animator => animator;

    private void Start()
    {
        _playerMovement = GetComponentInParent<PlayerMovementController>();

        if (_playerMovement == null)
            Debug.LogError("Missing player movement in parent");

        if (ClientBridge.Instance != null)
        {
            ClientBridge.Instance.OnClientPlayerReady += _ =>
            {
                ClientBridge.Instance.Stats.StatusChanged += HandleStatusChanged;
            };
        }
    }

    private void HandleStatusChanged(GameTag status, bool isActive)
    {
        if (isActive)
            _activeStatuses.Add(status);
        else
            _activeStatuses.Remove(status);

        if (_activeStatuses.Contains(GameTags.StatusStun))
        {
            animator.CrossFade(ImmobilizationHash, TransitionDuration);
        }
        else
        {
            animator.CrossFade(LocomotionHash, TransitionDuration);
        }
    }

    public void StopCurrentAnimation()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        if (_graph.IsValid())
            _graph.Destroy();

        animator.CrossFade(LocomotionHash, TransitionDuration, 0);
    }

    public void SetAnimationState(CharacterAnimationState state)
    {
        if (state == CharacterAnimationState.Unset)
            return;

        int hash = state switch
        {
            CharacterAnimationState.Locomotion => LocomotionHash,
            CharacterAnimationState.Immobilized => ImmobilizationHash,
            CharacterAnimationState.Dead => DeadHash,
            _ => 0
        };

        if (hash == 0)
        {
            Debug.LogWarning($"Unhandled animation state: {state}");
            return;
        }

        animator.CrossFade(hash, TransitionDuration);
    }

    public void PlayAttackAnimation(AttackAnimation attackAnimation, Action onFinished)
    {
        throw new NotImplementedException();
    }

    public void SetSpeed(float value)
    {
        throw new NotImplementedException();
    }


    public float PlayDodgeRoll()
    {
        PlayClip(dodgeAnimation, dodgeAnimation.length);
        return dodgeAnimation.length;
    }

    public Action PlayAnimation(AnimationClip clip, float duration)
    {
        PlayClip(clip, duration);
        return StopCurrentAnimation;
    }

    private void PlayClip(
        AnimationClip clip,
        float duration)
    {
        StopCurrentAnimation();

        if (clip == null)
            return;

        _graph = PlayableGraph.Create("AnimationGraph");

        var controllerPlayable = AnimatorControllerPlayable.Create(
            _graph,
            animator.runtimeAnimatorController);

        var clipPlayable = AnimationClipPlayable.Create(_graph, clip);

        _mixer = AnimationMixerPlayable.Create(_graph, 2);
        _mixer.ConnectInput(0, controllerPlayable, 0);
        _mixer.ConnectInput(1, clipPlayable, 0);

        _mixer.SetInputWeight(0, 1f);
        _mixer.SetInputWeight(1, 0f);

        var output = AnimationPlayableOutput.Create(
            _graph,
            "Animation",
            animator);

        output.SetSourcePlayable(_mixer);
        _graph.Play();

        _routine = StartCoroutine(
            PlayClipRoutine(duration, 0.15f));
    }

    private IEnumerator PlayClipRoutine(
        float duration,
        float fadeTime)
    {

        float time = 0f;

        while (time < fadeTime)
        {
            time += Time.deltaTime;
            float weight = time / fadeTime;

            _mixer.SetInputWeight(0, 1f - weight);
            _mixer.SetInputWeight(1, weight);

            yield return null;
        }

        _mixer.SetInputWeight(0, 0f);
        _mixer.SetInputWeight(1, 1f);

        yield return new WaitForSeconds(
            Mathf.Max(0f, duration - fadeTime * 2f));

        time = 0f;

        while (time < fadeTime)
        {
            time += Time.deltaTime;
            float weight = time / fadeTime;

            _mixer.SetInputWeight(0, weight);
            _mixer.SetInputWeight(1, 1f - weight);

            yield return null;
        }

        StopCurrentAnimation();
    }

    private void OnDestroy()
    {
        if (_routine != null)
            StopCoroutine(_routine);

        if (_graph.IsValid())
            _graph.Destroy();
    }
}