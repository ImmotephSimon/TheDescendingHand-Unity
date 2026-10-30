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
    [SerializeField] private float openSpeed = 1f;
    [SerializeField] private PropType propType;
    [SerializeField] private Collider interactionCollider;

    // Server raises this when the prop is opened.
    public event Action PlayerInteracted;

    private readonly SyncVar<bool> isOpen = new SyncVar<bool>();

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Quaternion targetRotation;

    private void Awake()
    {
        closedRotation = hinge.localRotation;
        openRotation = closedRotation * Quaternion.Euler(openAngle);
        targetRotation = closedRotation;

        if (interactionCollider == null)
        {
            Debug.LogError("HingeController requires an interaction collider assigned.", this);
            enabled = false;
            return;
        }

        if (propType == PropType.Door) interactionCollider.isTrigger = true;

        isOpen.OnChange += OnOpenChanged;
    }

    public override void OnStartServer()
    {
        if (propType == PropType.Chest)
            DungeonManager.Instance.RegisterDungeonChest(this);
    }

    // Server-only state changes
    [Server] public void Open() => isOpen.Value = true;
    [Server] public void Close() => isOpen.Value = false;

    private void OnOpenChanged(bool prev, bool next, bool asServer)
    {
        targetRotation = next ? openRotation : closedRotation;
        if (next && !prev && asServer) PlayerInteracted?.Invoke();
    }

    private void Update()
    {
        hinge.localRotation = Quaternion.Slerp(
            hinge.localRotation, targetRotation, openSpeed * Time.deltaTime);
    }

    // Doors: server-side triggers only
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized || propType != PropType.Door) return;
        if (!other.CompareTag("Player")) return;
        Open();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServerInitialized || propType != PropType.Door) return;
        if (!other.CompareTag("Player")) return;
        Close();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (propType != PropType.Chest) return;
        RequestOpenServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestOpenServerRpc()
    {
        Open();
    }
}