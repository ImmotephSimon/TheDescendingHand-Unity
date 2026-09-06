using FishNet;
using FishNet.Object;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class NetworkActionBase : NetworkBehaviour
{
    [SerializeField] private GameObject _cardPrefab;
    private IEntity _owner;
    private readonly Dictionary<Guid, GameObject> _activeClientVfx = new();

    private const int MaxSpawnedObjects = 50;
    private static readonly Queue<NetworkObject> _spawnedObjects = new();

    public override void OnStartServer()
    {
        base.OnStartServer();
        CardFactory.Initialize(
            registry: ClientBridge.Instance.CardRegistry,
            cardRuntimePrefab: _cardPrefab,
            serverNetworkSpawn: SpawnCapped,
            clientNetworkSpawn: SpawnClientVfx
        );
    }

    private void Awake()
    {
        _owner = GetComponent<IEntity>();
    }


    [Server]
    public CardInstance CreateCard(CardDefinition definition)
    {
        return CardFactory.CreateCardInstance(definition, _owner);
    }


    protected Action SpawnClientVfx(CardDefinition cardDefinition, VfxSpawnParams vfxSpawnParams)
    {
        Vector3 direction = (vfxSpawnParams.Position - _owner.Transform.position).normalized;
        direction.y = 0f;

        vfxSpawnParams.Rotation = Quaternion.LookRotation(direction);

        SpawnClientVfxObserversRpc(cardDefinition.Id, vfxSpawnParams);
        return () => StopClientVfx(vfxSpawnParams.InstanceId);
    }

    [ObserversRpc]
    private void SpawnClientVfxObserversRpc(Guid cardId, VfxSpawnParams vfxParams)
    {
        if (!ClientBridge.Instance.CardRegistry.TryGet(cardId, out CardDefinition def))
            return;

        if (def.Visuals.Vfx == null)
            return;

        GameObject vfx = def.Visuals.Vfx[vfxParams.VfxIndex];
        GameObject instance = Instantiate(vfx, vfxParams.Position, vfxParams.Rotation);
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
        if (!_activeClientVfx.Remove(instanceId, out GameObject instance))
            return;

        var vfx = instance.GetComponentInChildren<IVfx>();
        vfx?.Stop();
    }

    private static GameObject SpawnCapped(GameObject go)
    {
        InstanceFinder.ServerManager.Spawn(go);

        if (!go.TryGetComponent(out NetworkObject nob))
            return go;

        _spawnedObjects.Enqueue(nob);

        while (_spawnedObjects.Count > MaxSpawnedObjects)
        {
            var oldest = _spawnedObjects.Dequeue();

            if (oldest != null && oldest.IsSpawned)
                InstanceFinder.ServerManager.Despawn(oldest);
        }

        return go;
    }
}