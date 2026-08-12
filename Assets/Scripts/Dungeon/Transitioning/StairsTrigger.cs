using UnityEngine;
using System;

public class StairsTrigger : MonoBehaviour
{
    public event Action OnPlayerEnter;
    public event Action OnPlayerExit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) OnPlayerEnter?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) OnPlayerExit?.Invoke();
    }
}