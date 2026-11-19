
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HingeController : NetworkBehaviour, IPointerClickHandler
{
    public enum PropType { Door, Chest }

    [Header("References")]
    [SerializeField] private Transform hinge;

    [Header("Settings")]
    [SerializeField] private Vector3 openAngle = new Vector3(0, -150f, 0);
    [SerializeField] private float animationDuration = 0.4f;
    [SerializeField] private float startDelay = 0.15f;
    [SerializeField] private PropType propType;
    [SerializeField] private Collider interactionCollider;

    public event Action PlayerInteracted;
    public static event Action<HingeController, PropType> OnPropSpawned;

    // Server-authoritative state.
    private readonly SyncVar<bool> isOpen = new();
    private readonly SyncVar<bool> isReversed = new();
    private readonly SyncVar<double> transitionStartTime = new();
    private readonly SyncVar<Quaternion> transitionStartRotation = new();

    private double Now => TimeManager.TicksToTime(TimeManager.Tick);
    private Quaternion closedRotation;
    private Quaternion OpenRotation => closedRotation * Quaternion.Euler(isReversed.Value ? -openAngle : openAngle);

    private void Awake()
    {
        if (hinge == null || interactionCollider == null)
        {
            Debug.LogError("HingeController requires hinge and interactionCollider.", this);
            enabled = false;
            return;
        }

        closedRotation = hinge.localRotation;

        if (propType == PropType.Door)
            interactionCollider.isTrigger = true;

        isOpen.OnChange += OnOpenChanged;
    }

    public override void OnStartServer()
    {
        transitionStartRotation.Value = closedRotation;
        transitionStartTime.Value = Now;

        OnPropSpawned?.Invoke(this, propType);
    }

    public override void OnStopNetwork()
    {
        isOpen.OnChange -= OnOpenChanged;
    }

    // Server-authoritative state changes.

    [Server]
    public void Open(Transform player)
    {
        SetOpen(true, player);
    }

    [Server]
    public void Open() => SetOpen(true, null);

    [Server]
    public void Close() => SetOpen(false, null);

    [Server]
    private void SetOpen(bool open, Transform player)
    {
        if (isOpen.Value == open)
            return;

        if (open && propType == PropType.Door && player != null)
        {
            Vector3 toPlayer = player.position - hinge.position;
            float side = Vector3.Dot(hinge.forward, toPlayer);
            isReversed.Value = (side >= 0f);
        }

        Quaternion currentRotation = EvaluateRotation(Now);

        transitionStartRotation.Value = currentRotation;
        transitionStartTime.Value = Now + startDelay;
        isOpen.Value = open;
    }

    private Quaternion EvaluateRotation(double networkTime)
    {
        double elapsed = networkTime - transitionStartTime.Value;

        if (elapsed <= 0)
            return transitionStartRotation.Value;

        float progress = Mathf.Clamp01((float)(elapsed / animationDuration));

        progress = Mathf.SmoothStep(0f, 1f, progress);

        Quaternion destination = isOpen.Value
            ? OpenRotation
            : closedRotation;

        return Quaternion.Slerp(
            transitionStartRotation.Value,
            destination,
            progress
        );
    }

    private void OnOpenChanged(bool prev, bool next, bool asServer)
    {
        if (next && !prev && asServer)
            PlayerInteracted?.Invoke();
    }

    private void Update()
    {
        if (!IsServerInitialized && !IsClientInitialized)
            return;

        hinge.localRotation = EvaluateRotation(Now);
    }

    // Server-side door triggers.

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized || propType != PropType.Door)
            return;

        if (!other.CompareTag("Player"))
            return;

        Open(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServerInitialized || propType != PropType.Door)
            return;

        if (!other.CompareTag("Player"))
            return;

        Close();
    }

    // Chest interaction.

    public void OnPointerClick(PointerEventData eventData)
    {
        if (propType != PropType.Chest)
            return;

        RequestOpenServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestOpenServerRpc()
    {
        Open();
    }
}
