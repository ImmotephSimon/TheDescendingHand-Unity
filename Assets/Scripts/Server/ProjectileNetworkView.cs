using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEngine;

public class ProjectileNetworkView : NetworkBehaviour
{
    private readonly SyncVar<string> _visualId = new();

    public string VisualId => _visualId.Value;

    public event Action ServerStarted;
    public event Action ServerStopped;

    public event Action<string> ClientStarted;
    public event Action ClientStopped;
    public event Action ClientRedirected;
    public event Action ClientTerminalHit;

    // Set by the server before spawning the projectile.
    public void SetInitialVisualId(string id)
    {
        _visualId.Value = id;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        ServerStarted?.Invoke();
    }

    public override void OnStopServer()
    {
        ServerStopped?.Invoke();
        base.OnStopServer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        ClientStarted?.Invoke(_visualId.Value);
    }

    public override void OnStopClient()
    {
        ClientStopped?.Invoke();
        base.OnStopClient();
    }

    public void SendRedirect(Vector3 point, Quaternion rotation)
    {
        if (!IsServerInitialized)
            return;

        RpcRedirect(point, rotation);
    }

    [ObserversRpc]
    private void RpcRedirect(Vector3 point, Quaternion rotation)
    {
        transform.SetPositionAndRotation(point, rotation);
        ClientRedirected?.Invoke();
    }

    public void SendTerminalHit()
    {
        if (!IsServerInitialized)
            return;

        RpcTerminalHit();
    }

    [ObserversRpc]
    private void RpcTerminalHit()
    {
        ClientTerminalHit?.Invoke();
    }

    public void DespawnOnServer()
    {
        if (IsServerInitialized)
            Despawn();
    }
}