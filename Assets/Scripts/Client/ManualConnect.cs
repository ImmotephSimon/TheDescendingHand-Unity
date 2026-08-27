using UnityEngine;
using FishNet;
using FishNet.Transporting;
using UnityEngine.InputSystem;

public class ManualConnect : MonoBehaviour
{
    private string _status = "Press J to connect";

    void Update()
    {
        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            _status = InstanceFinder.ClientManager.StartConnection() ? "Connecting..." : "Failed to trigger connect";
        }
    }

    void OnEnable()
    {
        if (InstanceFinder.ClientManager != null) { 
        InstanceFinder.ClientManager.OnClientConnectionState += s => _status = $"State: {s.ConnectionState}";
        }
    }
}