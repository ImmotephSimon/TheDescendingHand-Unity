using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovementNetwork : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private EnemyBrain brain;
    [SerializeField] private float speedSmoothing = 10f;

    private float clientSmoothSpeed;
    private IEnemyMovement movementController;
    private readonly SyncVar<float> networkSpeed = new();
    private Component _animatorComponent;   // For unity-safe null check
    private IMovementAnimator _animator;

    public float CurrentSpeed => networkSpeed.Value;

    public delegate void StateChangedHandler(CharacterAnimationState newState);

    private void Awake()
    {
        movementController = GetComponent<IEnemyMovement>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        ((MonoBehaviour)movementController).enabled = true;
        agent.enabled = true;
        brain.enabled = true;
    }

    private void Update()
    {
        UpdateServerMovementState();
        UpdateClientVisuals();
    }

    private void UpdateServerMovementState()
    {
        if (!base.IsServerInitialized)
            return;

        float speed = agent.velocity.magnitude;

        networkSpeed.Value = speed;
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