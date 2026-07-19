using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour, IPlayerMovement
{
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
        public bool WantToCancel;

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    public struct ReconcileData : IReconcileData
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public Vector3 CurrentMoveVelocity;
        public MovementAction CurrentAction;
        public float ActionTimer;

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;

        public ReconcileData(Vector3 position, Vector3 velocity, Vector3 currentMoveVelocity, MovementAction currentAction, float actionTimer, uint tick)
        {
            Position = position;
            Velocity = velocity;
            CurrentMoveVelocity = currentMoveVelocity;
            CurrentAction = currentAction;
            ActionTimer = actionTimer;
            _tick = tick;
        }
    }

    private MovementLogicProcessor _processor = new MovementLogicProcessor();
    private CharacterController _controller;
    private Animator _animator;
    private PlayerStats _stats;

    private Vector3 _velocity;
    private Vector3 _currentMoveVelocity;
    private Vector2 _bufferedInput;
    private Vector3 _bufferedMousePos;

    public Vector3 Position => transform.position;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _stats = GetComponent<PlayerStats>();

        // Ensure the build processes network ticks properly even when alt-tabbed
        Application.runInBackground = true;
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
        if (IsOwner)
        {
            MoveData md = new MoveData
            {
                Input = _bufferedInput,
                MouseWorldPosition = _bufferedMousePos
            };

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
        {
            CreateReconcile();
        }
    }

    [Replicate]
    private void MoveCharacter(MoveData md, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        float delta = (float)TimeManager.TickDelta;

        // 1. Rotation Check
        Vector3 direction = md.MouseWorldPosition - transform.position;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(direction);

        // 2. Raw Input Translation
        Vector3 movementInput = new Vector3(md.Input.x, 0, md.Input.y).normalized;

        // 3. Gravity Check
        bool grounded = _controller.isGrounded;
        if (grounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }
        else
        {
            _velocity.y += _stats.Gravity.Value * delta;
        }

        // 4. Acceleration Diagnostics
        float moveSpeed = _stats.MoveSpeed.Value > 0 ? _stats.MoveSpeed.Value : 1f;
        Vector3 targetVelocity = movementInput * moveSpeed;

        float acceleration = 20f;
        float deceleration = 200f;
        float rate = movementInput == Vector3.zero ? deceleration : acceleration;

        _currentMoveVelocity = Vector3.MoveTowards(_currentMoveVelocity, targetVelocity, rate * delta);

        // 5. Final Motion Compilation
        Vector3 finalMotion = (_currentMoveVelocity + new Vector3(0, _velocity.y, 0)) * delta;

        _controller.Move(finalMotion);

    }

    [Reconcile]
    private void Reconciliation(ReconcileData rd, Channel channel = Channel.Unreliable)
    {
        transform.position = rd.Position;
        _velocity = rd.Velocity;
        _currentMoveVelocity = rd.CurrentMoveVelocity;
    }

    public override void CreateReconcile()
    {
        ReconcileData rd = new ReconcileData(
            transform.position,
            _velocity,
            _currentMoveVelocity,
            _processor.CurrentAction,
            _processor.ActionTimer,
            InstanceFinder.TimeManager.Tick
        );

        Reconciliation(rd);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        Debug.Log($"{name} IsOwner={IsOwner} IsServer={IsServerInitialized}");
    }

    private void Update()
    {


        float currentVelocity = _currentMoveVelocity.magnitude;
        _animator.SetFloat("Speed", currentVelocity);
        _animator.SetFloat("AnimSpeed", currentVelocity > 0.1f ? currentVelocity / _stats.MoveSpeed.Value : 1f);

        Vector3 movementInput = new Vector3(_bufferedInput.x, 0, _bufferedInput.y).normalized;
        Vector3 localMovement = transform.InverseTransformDirection(movementInput);
        _animator.SetFloat("MoveX", localMovement.x, 0.15f, Time.deltaTime);
        _animator.SetFloat("MoveY", localMovement.z, 0.15f, Time.deltaTime);
    }
}