using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HingeController : MonoBehaviour, IPointerClickHandler
{
    public enum PropType
    {
        Door,
        Chest
    }

    [Header("References")]
    [SerializeField] private Transform hinge;

    [Header("Settings")]
    [SerializeField] private Vector3 openAngle = new Vector3(0, -150f, 0);
    [SerializeField] private float openSpeed = 1f;
    [SerializeField] private PropType propType;
    [SerializeField] private Collider interactionCollider;

    public event Action PlayerInteracted;

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
    }

    private void Start()
    {
        if (propType == PropType.Chest) DungeonManager.Instance.RegisterDungeonChest(this);
    }

    private void Update()
    {
        hinge.localRotation = Quaternion.Slerp(
            hinge.localRotation,
            targetRotation,
            openSpeed * Time.deltaTime);
    }

    public void Open()
    {
        PlayerInteracted?.Invoke();
        targetRotation = openRotation;
    }

    public void Close()
    {
        targetRotation = closedRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (propType != PropType.Door)
            return;

        if (!other.CompareTag("Player"))
            return;

        Open();
    }

    private void OnTriggerExit(Collider other)
    {
        if (propType != PropType.Door)
            return;

        if (!other.CompareTag("Player"))
            return;

        Close();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (propType != PropType.Chest)
            return;

        Open();
    }
}