using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayerAnimationController : MonoBehaviour, IAnimationHandler
{
    [SerializeField] private AnimationClip castAnimationClip;
    [SerializeField] private AnimationClip specialCastAnimationClip;
    [SerializeField] private Animator animator;

    private PlayableGraph _graph;
    private AnimationClipPlayable _clipPlayable;
    private AnimationMixerPlayable _mixer;
    private Coroutine _routine;
    private float transitionDuration = 0.1f;
    private PlayerMovementController playerMovement;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
    private static readonly int DeadHash = Animator.StringToHash("Dead");
    private static readonly int StunHash = Animator.StringToHash("Stunned");
    private readonly Dictionary<CharacterAnimationState, int> animationHashes = new()
    {
        { CharacterAnimationState.Locomotion, LocomotionHash },
        { CharacterAnimationState.Stun, StunHash },
        { CharacterAnimationState.Dead, DeadHash }
    };


    private void Start()
    {
        ClientBridge.Instance.VFXView.SetAnimationHandler(this);
        playerMovement = GetComponentInParent<PlayerMovementController>();
        if (playerMovement == null) Debug.LogError($"Missing player movement in parent");
    }

    public void PlayAnimation(CardCastAnimation animation)
    {
        switch (animation)
        {
            case CardCastAnimation.Default:
                PlayAnimation(castAnimationClip, 1f);
                break;

            case CardCastAnimation.Special:
                PlayAnimation(specialCastAnimationClip, 1f);
                break;
        }
    }

    public void PlayAnimation(AnimationClip clip, float duration, Action onFinished = null)
    {
        StopCurrent();

        if (clip == null)
        {
            Debug.LogWarning("Tried to play null animation clip.");
            return;
        }

        _graph = PlayableGraph.Create("CardCastGraph");

        var controllerPlayable = AnimatorControllerPlayable.Create(
            _graph,
            animator.runtimeAnimatorController);

        _clipPlayable = AnimationClipPlayable.Create(_graph, clip);

        _mixer = AnimationMixerPlayable.Create(_graph, 2);

        _mixer.ConnectInput(0, controllerPlayable, 0);
        _mixer.ConnectInput(1, _clipPlayable, 0);

        _mixer.SetInputWeight(0, 1f);
        _mixer.SetInputWeight(1, 0f);

        var output = AnimationPlayableOutput.Create(
            _graph,
            "Animation",
            animator);

        output.SetSourcePlayable(_mixer);

        _graph.Play();

        _routine = StartCoroutine(PlayCastRoutine(duration, 0.15f, onFinished));
    }

    private IEnumerator PlayCastRoutine(float duration, float fadeTime, Action callback)
    {
        playerMovement.LockMovement();
        // Fade into cast
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

        // Hold cast
        yield return new WaitForSeconds(Mathf.Max(0f, duration - fadeTime * 2f));

        // Fade back
        time = 0f;

        while (time < fadeTime)
        {
            time += Time.deltaTime;

            float weight = time / fadeTime;

            _mixer.SetInputWeight(0, weight);
            _mixer.SetInputWeight(1, 1f - weight);

            yield return null;
        }

        StopCurrent();

        callback?.Invoke();
        playerMovement.UnlockMovement();
    }

    private void StopCurrent()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        if (_graph.IsValid())
        {
            _graph.Destroy();
        }
    }

    private void OnDestroy()
    {
        StopCurrent();
    }


    public void SetAnimationState(CharacterAnimationState state)
    {
        if (state is CharacterAnimationState.Unset)
            return;

        if (animationHashes.TryGetValue(state, out var hash))
        {
            animator.CrossFade(hash, transitionDuration);
        }
        else
        {
            Debug.LogWarning($"Unhandled animation state: {state}");
        }
    }

    public void PlayAnimation(AttackAnimation attackAnimation, Action onFinished)
    {
        throw new NotImplementedException();
    }

    public void SetSpeed(float clientSmoothSpeed)
    {
        throw new NotImplementedException();
    }
}