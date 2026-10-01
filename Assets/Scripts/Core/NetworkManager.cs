using FishNet;
using FishNet.Object;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : NetworkBehaviour
{
    public static NetworkManager Instance { get; private set; }

    [SerializeField] private GameObject _cardPrefab;
    public GameObject CardPrefab => _cardPrefab;

    private readonly Dictionary<Guid, GameObject> _activeClientVfx = new();

    private const int MaxSpawnedObjects = 50;
    private static readonly Queue<NetworkObject> _spawnedObjects = new();

    private void Awake()
    {
        Instance = this;
    }

    public GameObject SpawnCapped(GameObject go)
    {
        InstanceFinder.ServerManager.Spawn(go);

        if (!go.TryGetComponent(out NetworkObject nob)) return go;

        _spawnedObjects.Enqueue(nob);

        while (_spawnedObjects.Count > MaxSpawnedObjects)
        {
            var oldest = _spawnedObjects.Dequeue();

            if (oldest != null && oldest.IsSpawned)
                InstanceFinder.ServerManager.Despawn(oldest);
        }

        return go;
    }

    public Action SpawnClientVfx(IEntity owner, CardDefinition cardDefinition, VfxSpawnParams vfxSpawnParams)
    {
        Vector3 direction = (vfxSpawnParams.Position - owner.Transform.position).normalized;
        direction.y = 0f;

        vfxSpawnParams.Rotation = Quaternion.LookRotation(direction);

        NetworkObject attachTarget = owner.Transform.GetComponent<NetworkObject>();
        SpawnClientVfxObserversRpc(cardDefinition.Id, vfxSpawnParams, attachTarget);
        return () => StopClientVfx(vfxSpawnParams.InstanceId);
    }

    [ObserversRpc]
    private void SpawnClientVfxObserversRpc(Guid cardId, VfxSpawnParams vfxParams, NetworkObject attachTarget)
    {
        if (!ClientBridge.Instance.CardRegistry.TryGet(cardId, out CardDefinition def))
            return;

        if (def.Visuals.Vfx == null)
            return;

        GameObject vfx = def.Visuals.Vfx[vfxParams.VfxIndex];
        GameObject instance = vfxParams.Attach && attachTarget != null
            ? Instantiate(vfx, attachTarget.transform, false)
            : Instantiate(vfx, vfxParams.Position, vfxParams.Rotation);
        instance.transform.localScale = vfxParams.Scale;

        var controller = instance.GetComponentInChildren<IVfx>();
        controller?.Initialize(vfxParams);
        _activeClientVfx[vfxParams.InstanceId] = instance;
    }

    public void StopClientVfx(Guid instanceId)
    {
        StopClientVfxObserversRpc(instanceId);
    }

    [ObserversRpc]
    private void StopClientVfxObserversRpc(Guid instanceId)
    {
        if (!_activeClientVfx.Remove(instanceId, out GameObject instance)) return;

        var vfx = instance.GetComponentInChildren<IVfx>();
        vfx?.Stop();
    }
}