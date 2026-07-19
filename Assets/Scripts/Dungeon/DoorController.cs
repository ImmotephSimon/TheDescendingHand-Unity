using System;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform hinge;

    [Header("Settings")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 5f;
    [SerializeField] private Collider triggerCollider;

    public event Action PlayerEntered;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Quaternion targetRotation;


    private void Awake()
    {
        closedRotation = hinge.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);

        if (triggerCollider == null)
        {
            Debug.LogError("DoorController requires a trigger collider assigned.", this);
            enabled = false;
            return;
        }

        triggerCollider.isTrigger = true;
        targetRotation = closedRotation;
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
        targetRotation = openRotation;
    }

    public void Close()
    {
        targetRotation = closedRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        if (!other.CompareTag("Player"))
            return;

        PlayerEntered?.Invoke();
        Open();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Close();
    }

}