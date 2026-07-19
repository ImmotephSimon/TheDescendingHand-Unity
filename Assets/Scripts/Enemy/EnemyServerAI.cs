using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.LowLevel;

public class EnemyServerAI : NetworkBehaviour, IAnimationHandler
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private EnemyBrain brain;
    [SerializeField] private EnemyAnimationController animationController;
    [SerializeField] private EnemyMovementController movementController;

    private readonly SyncVar<CharacterAnimationState> currentNetworkState = new();
    private readonly SyncVar<float> networkSpeed = new();
    
    public CharacterAnimationState CurrentAnimationState => currentNetworkState.Value;
    public float CurrentSpeed => networkSpeed.Value;

    public delegate void StateChangedHandler(CharacterAnimationState newState);

    public override void OnStartServer()
    {
        base.OnStartServer();
        movementController.Initialize(agent, transform);


        

        agent.enabled = true;
        brain.enabled = true;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        currentNetworkState.OnChange += OnStateSyncChanged;

        if (!base.IsClientStarted)
            return;

        animationController.PlayStateDelayed(CurrentAnimationState);
    }

    public override void OnStopClient()
    {
        currentNetworkState.OnChange -= OnStateSyncChanged;

        base.OnStopClient();
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

    [SerializeField] private float speedSmoothing = 10f;
    private float clientSmoothSpeed;
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

    private void OnStateSyncChanged(CharacterAnimationState prev, CharacterAnimationState next, bool asServer)
    {
        if (asServer) return;
        Debug.Log($"Animation state {prev} -> {next}");
        animationController.PlayState(next);
    }
    public void SetAnimationState(CharacterAnimationState state)
    {
        if (!base.IsServerInitialized)
            return;

        Debug.Log($"Setting animation state: {state}");

        currentNetworkState.Value = state;
    }
    public void PlayAnimation(AnimationClip clip, Action onFinished)
    {
        animationController.PlayAnimation(clip, onFinished);
    }


}