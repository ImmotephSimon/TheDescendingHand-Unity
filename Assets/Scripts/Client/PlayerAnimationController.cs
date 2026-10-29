using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayerAnimationController : MonoBehaviour, IPlayerAnimationHandler
{
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip dodgeAnimation;

    private PlayableGraph _graph;
    private AnimationMixerPlayable _mixer;
    private Coroutine _routine;
    private const float BlendTime = 0.15f;
    private PlayerMovementController _playerMovement;
    private AnimationClipPlayable _activeClip;
    private PlayerStatsNetwork _stats;
    private static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
    private static readonly int DeadHash = Animator.StringToHash("Dead");
    private static readonly int ImmobilizationHash = Animator.StringToHash("Immobilized");
    private const float AnimationReferenceSpeed = 4f;

    private readonly HashSet<GameTag> _activeStatuses = new();

    public Animator Animator => animator;

    private void Start()
    {
        ClientRoot.Instance.OnClientPlayerReady += OnPlayerReady;
    }

    private void OnPlayerReady(ClientPlayer player)
    {
        _playerMovement = GetComponentInParent<PlayerMovementController>();
        _stats = GetComponentInParent<PlayerStatsNetwork>();

        if (_playerMovement == null)
            Debug.LogError("Missing player movement in parent", this);

        if (_stats == null)
        {
            Debug.LogError("Missing PlayerStatsNetwork", this);
            return;
        }

        _stats.StatusChanged += HandleStatusChanged;
    }

    private void OnDestroy()
    {
        if (_routine != null)
            StopCoroutine(_routine);

        if (_graph.IsValid())
            _graph.Destroy();

        if (ClientRoot.Instance != null)
            ClientRoot.Instance.OnClientPlayerReady -= OnPlayerReady;

        if (_stats != null)
            _stats.StatusChanged -= HandleStatusChanged;
    }

    private void HandleStatusChanged(GameTag status, bool isActive)
    {
        if (isActive)
            _activeStatuses.Add(status);
        else
            _activeStatuses.Remove(status);

        if (_activeStatuses.Contains(GameTags.StatusStun))
        {
            animator.CrossFade(ImmobilizationHash, BlendTime);
        }
        else
        {
            animator.CrossFade(LocomotionHash, BlendTime);
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

        //animator.CrossFade(LocomotionHash, BlendTime, 0);
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

        animator.CrossFade(hash, BlendTime);
    }

    public void PlayAttackAnimation(AttackAnimation attackAnimation, Action onFinished)
    {
        throw new NotImplementedException();
    }

    public void SetSpeed(float value)
    {
        throw new NotImplementedException();
    }


    public float PlayDodgeRoll(float duration)
    {
        PlayClip(dodgeAnimation, duration);
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

        _activeClip = AnimationClipPlayable.Create(_graph, clip);
        _activeClip.SetSpeed(clip.length / duration);

        _mixer = AnimationMixerPlayable.Create(_graph, 2);
        _mixer.ConnectInput(0, controllerPlayable, 0);
        _mixer.ConnectInput(1, _activeClip, 0);

        _mixer.SetInputWeight(0, 1f);
        _mixer.SetInputWeight(1, 0f);

        var output = AnimationPlayableOutput.Create(
            _graph,
            "Animation",
            animator);

        output.SetSourcePlayable(_mixer);
        _graph.Play();

        _routine = StartCoroutine(
            AnimationRoutine(duration));
    }

    private IEnumerator AnimationRoutine(
        float duration)
    {
        float time = 0f;

        while (time < BlendTime)
        {
            time += Time.deltaTime;
            float weight = time / BlendTime;

            _mixer.SetInputWeight(0, 1f - weight);
            _mixer.SetInputWeight(1, weight);

            yield return null;
        }

        _mixer.SetInputWeight(0, 0f);
        _mixer.SetInputWeight(1, 1f);

        yield return new WaitForSeconds(
            Mathf.Max(0f, duration));

        _activeClip.SetTime(_activeClip.GetDuration());
        _activeClip.SetSpeed(0f);

        time = 0f;

        while (time < BlendTime)
        {
            time += Time.deltaTime;
            float weight = time / BlendTime;

            _mixer.SetInputWeight(0, weight);
            _mixer.SetInputWeight(1, 1f - weight);

            yield return null;
        }
        StopCurrentAnimation();

    }


    public void ResetVisualRotation()
    {
        transform.localRotation = Quaternion.identity;
    }

    public void UpdateMovementAnimation(
        Vector2 input,
        float currentVelocity,
        float deltaTime,
        bool isDodging)
    {
        animator.SetFloat("Speed", currentVelocity);
        animator.SetFloat(
            "AnimSpeed",
            Mathf.Min(2f, currentVelocity > 0.1f
                ? currentVelocity / AnimationReferenceSpeed
                : 1f));

        if (isDodging)
        {
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", 1f);
            return;
        }

        Vector3 movementInput = new Vector3(input.x, 0f, input.y).normalized;
        Vector3 localMovement = transform.root.InverseTransformDirection(movementInput);

        animator.SetFloat("MoveX", localMovement.x, 0.15f, deltaTime);
        animator.SetFloat("MoveY", localMovement.z, 0.15f, deltaTime);
    }
}