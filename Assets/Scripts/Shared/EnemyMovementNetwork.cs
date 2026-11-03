// Game.Shared: lives on the base prefab
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class EnemyMovementNetwork : NetworkBehaviour
{
    [SerializeField] private float speedSmoothing = 10f;

    private readonly SyncVar<float> networkSpeed = new();
    private float clientSmoothSpeed;
    private Component _animatorComponent;   // For unity-safe null check
    private IMovementAnimator _animator;
    private IEnemyMovementState movementController;   // null on the client

    public float CurrentSpeed => networkSpeed.Value;

    private void Awake()
    {
        movementController = GetComponent<IEnemyMovementState>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        movementController?.Activate();
    }

    private void Update()
    {
        UpdateServerMovementState();
        UpdateClientVisuals();
    }

    private void UpdateServerMovementState()
    {
        if (!IsServerInitialized || movementController == null) return;

        networkSpeed.Value = movementController.Speed;
    }

    private void UpdateClientVisuals()
    {
        if (!IsClientStarted) return;

        if (_animatorComponent == null)
        {
            _animator = GetComponentInChildren<IMovementAnimator>();
            _animatorComponent = _animator as Component;
            if (_animatorComponent == null) return;
        }

        clientSmoothSpeed = Mathf.Lerp(
            clientSmoothSpeed, CurrentSpeed,
            1f - Mathf.Exp(-speedSmoothing * Time.deltaTime));

        _animator.SetSpeed(clientSmoothSpeed);
    }
}