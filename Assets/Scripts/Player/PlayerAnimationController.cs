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
    private AnimationClipPlayable _clipPlayable;
    private AnimationMixerPlayable _mixer;
    private Coroutine _routine;
    private float transitionDuration = 0.1f;
    private PlayerMovementController playerMovement;
    
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
    private static readonly int DeadHash = Animator.StringToHash("Dead");
    private static readonly int StunHash = Animator.StringToHash("Stunned");
    private static readonly int CastHash = Animator.StringToHash("Cast");
    private static readonly int SpecialCastHash = Animator.StringToHash("SpecialCast");

    public Animator Animator => animator;

    private readonly Dictionary<CharacterAnimationState, int> animationHashes = new()
    {
        { CharacterAnimationState.Locomotion, LocomotionHash },
        { CharacterAnimationState.Stun, StunHash },
        { CharacterAnimationState.Dead, DeadHash }
    };

    private void Start()
    {
        if (FishNet.InstanceFinder.IsServerStarted && !FishNet.InstanceFinder.IsClientStarted)
        {
            enabled = false;
            return;
        }

        playerMovement = GetComponentInParent<PlayerMovementController>();
        if (playerMovement == null) Debug.LogError("Missing player movement in parent");
    }

    // --- IAnimationHandler Implementation ---

    public void PlayCastAnimation(CardCastAnimation animation)
    {
        int hash = animation switch
        {
            CardCastAnimation.Default => CastHash,
            CardCastAnimation.Special => SpecialCastHash,
            _ => 0
        };

        if (hash == 0) return;

        if (animator == null)
        {
            Debug.LogError($"[{gameObject.name}] Animator reference is NULL!");
            return;
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError($"[{gameObject.name}] Animator has NO RuntimeAnimatorController assigned!");
            return;
        }

        if (!animator.HasState(0, hash))
        {
            Debug.LogError($"[{gameObject.name}] Controller '{animator.runtimeAnimatorController.name}' does NOT contain state hash '{hash}' on Layer 0!");
            return;
        }
        animator.CrossFade(hash, transitionDuration, 0);
    }

    public void StopCurrentAnimation()
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

        animator.CrossFade(LocomotionHash, transitionDuration, 0);
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

    public void PlayAttackAnimation(AttackAnimation attackAnimation, Action onFinished)
    {
        throw new NotImplementedException();
    }

    public void SetSpeed(float value)
    {
        throw new NotImplementedException();
    }

    // --- Playable Graph Clip Logic ---

    public void PlayAnimation(AnimationClip clip, float duration, Action onFinished = null)
    {
        PlayClip(clip, duration, true, onFinished);
    }

    public float PlayDodgeRoll()
    {
        PlayClip(dodgeAnimation, dodgeAnimation.length, false);
        return dodgeAnimation.length;
    }

    private void PlayClip(
        AnimationClip clip,
        float duration,
        bool lockMovement,
        Action onFinished = null)
    {
        StopCurrentAnimation();

        if (clip == null)
            return;

        _graph = PlayableGraph.Create("AnimationGraph");

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

        _routine = StartCoroutine(
            PlayClipRoutine(duration, 0.15f, lockMovement, onFinished));
    }

    private IEnumerator PlayClipRoutine(
        float duration,
        float fadeTime,
        bool lockMovement,
        Action callback)
    {
        if (lockMovement)
            playerMovement.LockMovement();

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
        callback?.Invoke();
    }

    private void OnDestroy()
    {
        if (_routine != null)
            StopCoroutine(_routine);

        if (_graph.IsValid())
            _graph.Destroy();
    }

    public void StopCastAnimation()
    {
        animator.CrossFade(LocomotionHash, transitionDuration, 0);
    }
}
