using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEngine;

public class ProjectileNetwork : NetworkBehaviour
{
    private readonly SyncVar<VfxType> _visual = new();

    public VfxType Visual => _visual.Value;

    public event Action ServerStarted;
    public event Action ServerStopped;

    public event Action ClientRedirected;
    public event Action ClientTerminalHit;

    public static event Action<ProjectileNetwork> AnyClientStarted;
    public static event Action<ProjectileNetwork> AnyClientStopped;

    public override void OnStartClient()
    {
        base.OnStartClient();
        AnyClientStarted?.Invoke(this);
    }

    public override void OnStopClient()
    {
        AnyClientStopped?.Invoke(this);
        base.OnStopClient();
    }

    // Set by the server before spawning the projectile.
    public void SetInitialVisualId(VfxType vfxType)
    {
        _visual.Value = vfxType;
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