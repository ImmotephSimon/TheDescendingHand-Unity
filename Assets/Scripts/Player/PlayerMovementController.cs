using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : NetworkBehaviour, IPlayerMovement
{
    private readonly float _accelerationMultiplier = 12f;
    private readonly float _decelerationMultiplier = 16f;

    [SerializeField] private AnimationCurve _dodgeSpeedCurve = new AnimationCurve(
        new Keyframe(0f, 0f),    // initial delay
        new Keyframe(0.15f, 0f), // still stationary
        new Keyframe(0.5f, 2f), // speed up
        new Keyframe(0.65f, 1f), // middle
        new Keyframe(1f, 0f));   // end
    public enum MovementAction : byte
    {
        None,
        DodgeRoll
    }

    public struct MoveData : IReplicateData
    {
        public Vector2 Input;
        public Vector3 MouseWorldPosition;
        public MovementAction ActionRequested;

        private uint _tick;

        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    public struct ReconcileData : IReconcileData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 CurrentMoveVelocity;
        public MovementAction CurrentAction;
        public float ActionTimer;
        public float DodgeDuration;

        private uint _tick;

        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;

        public ReconcileData(
            Vector3 position,
            Quaternion rotation,
            Vector3 velocity,
            Vector3 currentMoveVelocity,
            MovementAction currentAction,
            float actionTimer,
            float dodgeDuration,
            uint tick)
        {
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            CurrentMoveVelocity = currentMoveVelocity;
            CurrentAction = currentAction;
            ActionTimer = actionTimer;
            DodgeDuration = dodgeDuration;
            _tick = tick;
        }
    }

    private const float DodgeTurnSpeed = 180f;

    private CharacterController _controller;
    private PlayerAnimationController _animationHandler;
    private Animator _animator;
    private Transform _visual;

    private Vector3 _velocity;
    private Vector3 _currentMoveVelocity;
    private Vector2 _bufferedInput;
    private Vector3 _bufferedMousePos;

    private MovementAction _currentAction;
    private float _actionTimer;
    private float _dodgeDuration;

    private int _movementLocks;
    private IStatContainer _statContainer;
    private MovementAction _bufferedAction;
    const float AnimationReferenceSpeed = 4f;
    private readonly SyncVar<float> _networkedMoveSpeed = new(2.5f);

    public Vector3 CursorPosition => _bufferedMousePos;
    public bool CanMove => _movementLocks == 0;
    public Vector3 Position => transform.position;
    public float Gravity { get; private set; } = -9.81f;
    public float MoveSpeed => _networkedMoveSpeed.Value;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animationHandler = GetComponentInChildren<PlayerAnimationController>();
        _animator = _animationHandler.Animator;
        _visual = _animationHandler.transform;
        _statContainer = GetComponent<IStatContainer>();

        Debug.Assert(_statContainer != null, "No stat container.");

        Application.runInBackground = true;
        _animationHandler.SetAnimationState(CharacterAnimationState.Locomotion);
    }

    public void SetLocalInput(Vector2 input, Vector3 mousePos)
    {
        _bufferedInput = input;
        _bufferedMousePos = mousePos;
    }

    public override void OnStartNetwork()
    {
        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;

        if (IsServerStarted)
        {
            _statContainer.Listen(GameTags.ModStatMovement, OnMovementStatChanged);
            OnMovementStatChanged(
                _statContainer.GetStat(GameTags.ModStatMovement));
        }
    }

    private void OnMovementStatChanged(float newValue)
    {
        _networkedMoveSpeed.Value = newValue;
    }

    public override void OnStopNetwork()
    {
        if (InstanceFinder.TimeManager != null)
        {
            InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
            InstanceFinder.TimeManager.OnPostTick -= TimeManager_OnPostTick;
        }

        if (IsServerStarted && _statContainer != null)
        {
            _statContainer.StopListening(
                GameTags.ModStatMovement,
                OnMovementStatChanged);
        }
    }

    private void TimeManager_OnTick()
    {
        if (IsOwner)
        {
            MoveData md = new MoveData
            {
                Input = _bufferedInput,
                MouseWorldPosition = _bufferedMousePos,
                ActionRequested = _bufferedAction
            };

            //_bufferedAction = MovementAction.None;
            MoveCharacter(md);
        }
        else if (IsServerInitialized)
        {
            MoveCharacter(default);
        }
    }

    private void TimeManager_OnPostTick()
    {
        if (IsOwner)
            CreateReconcile();
    }

    [Replicate]
    private void MoveCharacter(
        MoveData md,
        ReplicateState state = ReplicateState.Invalid,
        Channel channel = Channel.Unreliable)
    {
        if (!CanMove || IsImmobilized())
            return;

        _bufferedMousePos = md.MouseWorldPosition;

        float delta = (float)TimeManager.TickDelta;
        Vector3 movementInput = new Vector3(md.Input.x, 0f, md.Input.y).normalized;

        ApplyGravity(delta);

        if (_currentAction == MovementAction.None &&
            md.ActionRequested == MovementAction.DodgeRoll)
        {
            StartDodgeRoll();

            if (IsOwner)
                _bufferedAction = MovementAction.None;
        }

        if (_currentAction == MovementAction.DodgeRoll)
        {
            ProcessDodgeRollAction(movementInput, delta);
        }
        else
        {
            ProcessLocomotion(movementInput, md.MouseWorldPosition);
        }

        Vector3 finalMotion =
            (_currentMoveVelocity + new Vector3(0f, _velocity.y, 0f)) * delta;

        _controller.Move(finalMotion);
    }

    private static bool IsImmobilized()
    {
        if (ClientBridge.Instance.Stats == null) return false;

        foreach (var tag in GameTags.Immobilizations)
        {
            if (ClientBridge.Instance.Stats.Statuses.Contains(tag.TagId))
                return true;
        }

        return false;
    }

    private void ApplyGravity(float delta)
    {
        if (_controller.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;
        else
            _velocity.y += Gravity * delta;
    }

    private void StartDodgeRoll()
    {
        _currentAction = MovementAction.DodgeRoll;
        _dodgeDuration = _animationHandler.PlayDodgeRoll();
        _actionTimer = _dodgeDuration;
    }

    private void ProcessDodgeRollAction(Vector3 movementInput, float delta)
    {
        _actionTimer -= delta;

        float progress = 1f - (_actionTimer / _dodgeDuration);
        float speedMultiplier = _dodgeSpeedCurve.Evaluate(progress);

        Vector3 moveDir = movementInput.sqrMagnitude > 0.001f
            ? movementInput
            : transform.forward;

        _currentMoveVelocity = moveDir * (MoveSpeed * speedMultiplier);

        if (movementInput.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                DodgeTurnSpeed * delta);
        }

        if (_actionTimer <= 0f)
        {
            _currentAction = MovementAction.None;
            _actionTimer = 0f;
            _currentMoveVelocity = Vector3.zero;
        }
    }

    private void ProcessLocomotion(Vector3 movementInput, Vector3 mouseWorldPosition)
    {
        Vector3 direction = mouseWorldPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);

        float moveSpeed = MoveSpeed;
        Vector3 targetVelocity = movementInput * moveSpeed;

        float rate = movementInput.sqrMagnitude > 0.001f
             ? MoveSpeed * _accelerationMultiplier
             : MoveSpeed * _decelerationMultiplier;

        _currentMoveVelocity = Vector3.MoveTowards(
            _currentMoveVelocity,
            targetVelocity,
            rate * (float)TimeManager.TickDelta);
    }

    [Reconcile]
    private void Reconciliation(
        ReconcileData rd,
        Channel channel = Channel.Unreliable)
    {
        transform.position = rd.Position;
        transform.rotation = rd.Rotation;
        _velocity = rd.Velocity;
        _currentMoveVelocity = rd.CurrentMoveVelocity;
        _currentAction = rd.CurrentAction;
        _actionTimer = rd.ActionTimer;
        _dodgeDuration = rd.DodgeDuration;
    }

    public override void CreateReconcile()
    {
        Reconciliation(
            new ReconcileData(
                transform.position,
                transform.rotation,
                _velocity,
                _currentMoveVelocity,
                _currentAction,
                _actionTimer,
                _dodgeDuration,
                InstanceFinder.TimeManager.Tick));
    }


    private void Update()
    {
        float currentVelocity = _currentMoveVelocity.magnitude;

        _animator.SetFloat("Speed", currentVelocity);
        _animator.SetFloat(
            "AnimSpeed", Mathf.Min(2f, 
            currentVelocity > 0.1f
                ? currentVelocity / AnimationReferenceSpeed
                : 1f));

        Vector3 movementInput =
            new Vector3(_bufferedInput.x, 0f, _bufferedInput.y).normalized;

        if (_currentAction == MovementAction.DodgeRoll)
        {
            _animator.SetFloat("MoveX", 0f);
            _animator.SetFloat("MoveY", 1f);
            return;
        }


        _visual.localRotation = Quaternion.identity;

        Vector3 localMovement =
            transform.InverseTransformDirection(movementInput);

        _animator.SetFloat(
            "MoveX",
            localMovement.x,
            0.15f,
            Time.deltaTime);

        _animator.SetFloat(
            "MoveY",
            localMovement.z,
            0.15f,
            Time.deltaTime);
    }

    public void DodgeRoll()
    {
        ClientBridge.Instance.AbilitySystem.RequestCancelCurrentCast();
        _bufferedAction = MovementAction.DodgeRoll;
    }
}