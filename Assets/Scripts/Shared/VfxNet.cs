using FishNet.Object;
using System;

public class VfxNet : NetworkBehaviour
{
    public static VfxNet Instance { get; private set; }

    public event Action<VfxString, VfxSpawnParams, NetworkObject> SpawnRequested;
    public event Action<Guid> StopRequested;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        Instance = this;
    }

    public override void OnStopNetwork()
    {
        if (Instance == this)
            Instance = null;

        base.OnStopNetwork();
    }

    /// <summary>
    /// Requests a VFX spawn on clients observing this object.
    /// Call from the server.
    /// </summary>
    public Action Spawn(
            VfxString vfxString,
            VfxSpawnParams parameters,
            NetworkObject attachTarget)
    {
        if (!IsServerInitialized)
            return null;

        RpcSpawn(vfxString, parameters, attachTarget);
        return () => Stop(parameters.InstanceId);
    }

    [ObserversRpc]
    private void RpcSpawn(
            VfxString vfxString,
            VfxSpawnParams parameters,
            NetworkObject attachTarget)
    {
        SpawnRequested?.Invoke(vfxString, parameters, attachTarget);
    }

    /// <summary>
    /// Stops a previously spawned VFX on observing clients.
    /// </summary>
    public void Stop(Guid instanceId)
    {
        if (!IsServerInitialized)
            return;

        RpcStop(instanceId);
    }

    [ObserversRpc]
    private void RpcStop(Guid instanceId)
    {
        StopRequested?.Invoke(instanceId);
    }
}