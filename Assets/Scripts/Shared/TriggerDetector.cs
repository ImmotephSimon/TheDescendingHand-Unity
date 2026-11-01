using FishNet.Object;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetector : MonoBehaviour
{
    private readonly Dictionary<NetworkObject, int> _inside = new();

    public event Action<NetworkObject> OnEntered;
    public event Action<NetworkObject> OnExited;

    private void OnTriggerEnter(Collider other)
    {
        var nob = other.GetComponentInParent<NetworkObject>();
        if (nob == null) return;

        _inside.TryGetValue(nob, out var n);
        _inside[nob] = n + 1;
        if (n == 0) OnEntered?.Invoke(nob);
    }

    private void OnTriggerExit(Collider other)
    {
        var nob = other.GetComponentInParent<NetworkObject>();
        if (nob == null || !_inside.TryGetValue(nob, out var n)) return;

        if (n > 1) { _inside[nob] = n - 1; return; }

        _inside.Remove(nob);
        OnExited?.Invoke(nob);
    }
}