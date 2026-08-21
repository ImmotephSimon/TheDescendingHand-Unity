using FishNet;
using FishNet.Connection;
using FishNet.Managing.Object;
using FishNet.Object;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private DualPrefabObjects _dualPrefabs;
    [SerializeField] private bool _addToDefaultScene = true;
    [SerializeField] private Transform[] _spawns;

    private int _nextSpawn;

    private void OnEnable()
    {
        InstanceFinder.SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;
    }

    private void OnDisable()
    {
        InstanceFinder.SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;
    }

    private void OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
    {
        if (!asServer)
            return;

        if (_dualPrefabs == null || _dualPrefabs.Prefabs.Count == 0)
        {
            Debug.LogError("No dual player prefab configured.");
            return;
        }

        NetworkObject serverPrefab = _dualPrefabs.Prefabs[0].Server;

        GetSpawnPosition(
            serverPrefab.transform,
            out Vector3 position,
            out Quaternion rotation);

        NetworkObject player = InstanceFinder.NetworkManager.GetPooledInstantiated(
            serverPrefab.PrefabId,
            serverPrefab.SpawnableCollectionId,
            position,
            rotation,
            true);

        if (player == null)
        {
            Debug.LogError("Failed to instantiate player prefab.");
            return;
        }
        Debug.Log(
    $"Spawned player: {player.name}, " +
    $"PrefabId={serverPrefab.PrefabId}, " +
    $"CollectionId={serverPrefab.SpawnableCollectionId}");


        InstanceFinder.ServerManager.Spawn(player, conn);

        if (_addToDefaultScene)
            InstanceFinder.SceneManager.AddOwnerToDefaultScene(player);
    }

    private void GetSpawnPosition(
        Transform prefab,
        out Vector3 position,
        out Quaternion rotation)
    {
        if (_spawns == null || _spawns.Length == 0)
        {
            position = prefab.position;
            rotation = prefab.rotation;
            return;
        }

        Transform spawn = _spawns[_nextSpawn];

        if (spawn == null)
        {
            position = prefab.position;
            rotation = prefab.rotation;
        }
        else
        {
            position = spawn.position;
            rotation = spawn.rotation;
        }

        _nextSpawn = (_nextSpawn + 1) % _spawns.Length;
    }
}