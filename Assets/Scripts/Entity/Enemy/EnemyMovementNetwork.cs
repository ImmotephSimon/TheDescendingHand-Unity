using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.LowLevel;

public class EnemyMovementNetwork : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private EnemyBrain brain;
    [SerializeField] private float speedSmoothing = 10f;

    private float clientSmoothSpeed;
    private IAnimationHandler animationController;
    private IEnemyMovement movementController;
    private readonly SyncVar<float> networkSpeed = new();
    
    public float CurrentSpeed => networkSpeed.Value;

    public delegate void StateChangedHandler(CharacterAnimationState newState);

    private void Awake()
    {
        animationController = GetComponentInChildren<IAnimationHandler>();
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
        if (!base.IsClientStarted)
            return;

        // Smoothly interpolate the animation speed to eliminate network tick snapping
        clientSmoothSpeed = Mathf.Lerp(clientSmoothSpeed, CurrentSpeed, Time.deltaTime * speedSmoothing);

        // Prevent microscopic float dragging near zero
        if (Mathf.Approximately(CurrentSpeed, 0f) && clientSmoothSpeed < 0.05f)
        {
            clientSmoothSpeed = 0f;
        }

        animationController.SetSpeed(clientSmoothSpeed);
    }

}