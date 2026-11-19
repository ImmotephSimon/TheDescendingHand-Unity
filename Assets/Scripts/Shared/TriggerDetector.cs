using FishNet.Object;
using System;
using UnityEngine;

public class TriggerDetector : MonoBehaviour
{
    public event Action<NetworkObject> OnStay;

    private void OnTriggerStay(Collider other)
    {
        var nob = other.GetComponentInParent<NetworkObject>();
        if (nob != null)
            OnStay?.Invoke(nob);
    }
}