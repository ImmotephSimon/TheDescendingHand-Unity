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

    public struct ClientMoveData : IReplicateData
    {
        public Vector2 Input;
        public Vector3 MouseWorldPosition;
        public MovementAction ActionRequested;
        public float MoveSpeed;
        public float DodgeSpeedMultiplier;

        private uint _tick;

        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    public struct ServerReconcileData : IReconcileData
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

        public ServerReconcileData(
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

    private Vector2 _ownerInput;
    private Vector3 _ownerMousePos;
    private MovementAction _ownerAction;

    private MovementAction _currentAction;
    private float _actionTimer;

    private PlayerStatsNetwork _stats;

    private Vector3 _sharedMousePos;
    public Vector3 CursorPosition => IsOwner ? _ownerMousePos : _sharedMousePos;
    public Vector3 Position => transform.position;
    public float Gravity { get; private set; } = -9.81f;

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
        _ownerMousePos = mousePos;
        _ownerInput = !IsImmobilized()
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
        MoveCharacter(Owner_BuildMoveData());
    }

    private ClientMoveData Owner_BuildMoveData()
    {
        if (!IsOwner)
        {
            //no local input to send on a non-owner
            return default;
        }
        return new ClientMoveData
        {
            Input = _ownerInput,
            MouseWorldPosition = _ownerMousePos,
            ActionRequested = _ownerAction,
            MoveSpeed = _stats.MoveSpeed.Value,
            DodgeSpeedMultiplier = _stats.DodgeSpeedMultiplier.Value
        };
    }

    private void TimeManager_OnPostTick()
    {
        if (IsServerInitialized)
            CreateReconcile();
    }

    [Replicate]
    private void MoveCharacter(
        ClientMoveData md,
        ReplicateState state = ReplicateState.Invalid,
        Channel channel = Channel.Unreliable)
    {
        float moveSpeed = md.MoveSpeed;
        if (IsServerInitialized)
            moveSpeed = Mathf.Min(moveSpeed, _stats.MoveSpeed.Value);

        if (IsImmobilized())
        {
            _currentMoveVelocity = Vector3.zero;
            return;
        }



        _sharedMousePos = md.MouseWorldPosition;

        float delta = (float)TimeManager.TickDelta;
        Vector3 movementInput = new Vector3(md.Input.x, 0f, md.Input.y).normalized;

        ApplyGravity(delta);

        if (_currentAction == MovementAction.None &&
            md.ActionRequested == MovementAction.DodgeRoll)
        {
            StartDodgeRoll();

            if (IsOwner)
                _ownerAction = MovementAction.None;
        }

        if (_currentAction == MovementAction.DodgeRoll)
            ProcessDodgeRollAction(movementInput, delta, moveSpeed);
        else
            ProcessLocomotion(movementInput, md.MouseWorldPosition, moveSpeed);


        Vector3 finalMotion =
            (_currentMoveVelocity + new Vector3(0f, _velocity.y, 0f)) * delta;

        _controller.Move(finalMotion);
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

    private void ProcessDodgeRollAction(Vector3 movementInput, float delta, float moveSpeed)
    {
        _actionTimer -= delta;


        float progress = 1f - (_actionTimer / DodgeDuration);
        float speedMultiplier = _dodgeSpeedCurve.Evaluate(progress);

        Vector3 moveDir = movementInput.sqrMagnitude > 0.001f
            ? movementInput
            : transform.forward;


        _currentMoveVelocity = moveDir * (moveSpeed * speedMultiplier);

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

    private void ProcessLocomotion(
    Vector3 movementInput,
    Vector3 mouseWorldPosition,
    float moveSpeed)
    {


        Vector3 direction = mouseWorldPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);

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
        ServerReconcileData server,
        Channel channel = Channel.Unreliable)
    {
        TeleportCharacterController(server.Position);

        _velocity = server.Velocity;
        _currentMoveVelocity = server.CurrentMoveVelocity;
        _currentAction = server.CurrentAction;
        _actionTimer = server.ActionTimer;
    }

    private void TeleportCharacterController(Vector3 position)
    {
        _controller.enabled = false;
        transform.position = position;
        _controller.enabled = true;
    }

    public override void CreateReconcile()
    {
        Reconciliation(
            new ServerReconcileData(
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
            _ownerInput,
            currentVelocity,
            Time.deltaTime,
            isDodging);

        Vector3 movementInput =
            new Vector3(_ownerInput.x, 0f, _ownerInput.y).normalized;
    }

    public void DodgeRoll()
    {
        GetComponent<CardNetwork>().RequestCancelCurrentCast();
        _ownerAction = MovementAction.DodgeRoll;
    }
}