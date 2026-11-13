using FishNet;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class ServerObjectSpawner : MonoBehaviour
{
    public static ServerObjectSpawner Instance { get; private set; }

    [SerializeField] private GameObject _cardPrefab;
    public GameObject CardPrefab => _cardPrefab;

    private const int MaxSpawnedObjects = 50;
    private readonly Queue<NetworkObject> _spawnedObjects = new();

    private void Awake()
    {
        Instance = this;
    }

    public GameObject SpawnCapped(GameObject go)
    {
        if (!go.TryGetComponent(out NetworkObject nob))
        {
            Debug.LogError($"ServerObjectSpawner: {go.name} has no NetworkObject.", go);
            return go;
        }

        InstanceFinder.ServerManager.Spawn(go);
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