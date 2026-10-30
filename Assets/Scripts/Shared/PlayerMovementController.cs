using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : NetworkBehaviour, IPlayerMovement
{
    private readonly float _accelerationMultiplier = 12f;
    private readonly float _decelerationMultiplier = 16f;
    private static readonly float _baseDodgeDuration = 2.033f;

    [SerializeField]
    private AnimationCurve _dodgeSpeedCurve = new AnimationCurve(
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
            uint tick)
        {
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            CurrentMoveVelocity = currentMoveVelocity;
            CurrentAction = currentAction;
            ActionTimer = actionTimer;
            _tick = tick;
        }
    }

    private const float DodgeTurnSpeed = 180f;

    private CharacterController _controller;
    private IPlayerAnimationHandler _animationHandler;

    private Vector3 _velocity;
    private Vector3 _currentMoveVelocity;
    private Vector2 _bufferedInput;
    private Vector3 _bufferedMousePos;

    private MovementAction _currentAction;
    private float _actionTimer;

    private PlayerStatsNetwork _stats;
    private MovementAction _bufferedAction;

    public Vector3 CursorPosition => _bufferedMousePos;
    public Vector3 Position => transform.position;
    public float Gravity { get; private set; } = -9.81f;
    private float MoveSpeed => _stats.MoveSpeed.Value;

    private float DodgeDuration =>
        _baseDodgeDuration / _stats.DodgeSpeedMultiplier.Value;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animationHandler = GetComponentInChildren<IPlayerAnimationHandler>();
        _stats = GetComponent<PlayerStatsNetwork>();

        Debug.Assert(_stats != null, "No stat container.");

        Application.runInBackground = true;
        _animationHandler.SetAnimationState(CharacterAnimationState.Locomotion);
    }

    public void SetLocalInput(Vector2 input, Vector3 mousePos)
    {
        _bufferedMousePos = mousePos;
        _bufferedInput = !IsImmobilized()
            ? input
            : Vector2.zero;
    }

    public override void OnStartNetwork()
    {
        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;

    }


    public override void OnStopNetwork()
    {
        if (InstanceFinder.TimeManager != null)
        {
            InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
            InstanceFinder.TimeManager.OnPostTick -= TimeManager_OnPostTick;
        }
    }

    private void TimeManager_OnTick()
    {
        MoveCharacter(BuildMoveData());
    }

    private MoveData BuildMoveData()
    {
        if (!IsOwner)
            return default;

        return new MoveData
        {
            Input = _bufferedInput,
            MouseWorldPosition = _bufferedMousePos,
            ActionRequested = _bufferedAction
        };
    }

    private void TimeManager_OnPostTick()
    {
        if (IsServerInitialized)
            CreateReconcile();
    }

    [Replicate]
    private void MoveCharacter(
        MoveData md,
        ReplicateState state = ReplicateState.Invalid,
        Channel channel = Channel.Unreliable)
    {

        if (IsImmobilized())
        {
            _currentMoveVelocity = Vector3.zero;
            return;
        }


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

        if (md.Input != Vector2.zero || _currentMoveVelocity.sqrMagnitude > 0.001f)
        {
            Debug.Log($"[Move] t={Time.time:F2} netId={ObjectId} owner={IsOwner} server={IsServerInitialized} " +
                      $"state={state} in={md.Input} vel={_currentMoveVelocity.magnitude:F2} " +
                      $"speed={MoveSpeed:F1} pos={transform.position}");
        }
    }

    private bool IsImmobilized()
    {
        foreach (var tag in GameTags.Immobilizations)
        {
            if (_stats.Statuses.Contains(tag.TagId))
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

        _animationHandler.PlayDodgeRoll(DodgeDuration);

        _actionTimer = DodgeDuration;
    }

    private void ProcessDodgeRollAction(Vector3 movementInput, float delta)
    {
        _actionTimer -= delta;


        float progress = 1f - (_actionTimer / DodgeDuration);
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

        if (moveSpeed <= 0f)
        {
            _currentMoveVelocity = Vector3.zero;
            return;
        }

        Vector3 targetVelocity = movementInput * moveSpeed;

        float rate = movementInput.sqrMagnitude > 0.001f
            ? moveSpeed * _accelerationMultiplier
            : moveSpeed * _decelerationMultiplier;

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
        _controller.enabled = false;
        transform.SetPositionAndRotation(rd.Position, rd.Rotation);
        _controller.enabled = true;

        _velocity = rd.Velocity;
        _currentMoveVelocity = rd.CurrentMoveVelocity;
        _currentAction = rd.CurrentAction;
        _actionTimer = rd.ActionTimer;
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
                InstanceFinder.TimeManager.Tick));
    }


    private void Update()
    {
        float currentVelocity = _currentMoveVelocity.magnitude;

        bool isDodging = _currentAction == MovementAction.DodgeRoll;

        _animationHandler.UpdateMovementAnimation(
            _bufferedInput,
            currentVelocity,
            Time.deltaTime,
            isDodging);

        Vector3 movementInput =
            new Vector3(_bufferedInput.x, 0f, _bufferedInput.y).normalized;
    }

    public void DodgeRoll()
    {
        GetComponent<CardNetwork>().RequestCancelCurrentCast();
        _bufferedAction = MovementAction.DodgeRoll;
    }
}