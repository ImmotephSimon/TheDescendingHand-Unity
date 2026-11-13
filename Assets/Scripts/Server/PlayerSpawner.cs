using FishNet;
using FishNet.Connection;
using FishNet.Managing.Object;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private DualPrefabObjects _dualPrefabs;

    private int _nextSpawn;
    private SceneManager _manager;

    private void OnEnable()
    {
        _manager = InstanceFinder.SceneManager;
        if (_manager == null) { Debug.LogError("SceneManager not ready."); return; }
        _manager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;
    }

    private void OnDisable()
    {
        if (_manager != null)
            _manager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;
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

        var coll = InstanceFinder.NetworkManager.SpawnablePrefabs;
        Debug.Log($"count={coll.GetObjectCount()} playerId={serverPrefab.PrefabId}");
        var found = coll.GetObject(true, serverPrefab.PrefabId);
        Debug.Log($"lookup -> {(found == null ? "NULL" : found.name)}");

        Transform spawn = GetSpawnPoint(serverPrefab.transform);

        NetworkObject player = Instantiate(serverPrefab, spawn.position, spawn.rotation);
        Debug.Log($"after Instantiate: self={player.gameObject.activeSelf} hierarchy={player.gameObject.activeInHierarchy}");

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
        InstanceFinder.SceneManager.AddOwnerToDefaultScene(player);
    }

    private Transform GetSpawnPoint(Transform prefab)
    {
        var points = GameObject.FindGameObjectsWithTag("SpawnPoint");

        if (points.Length == 0)
        {
            Debug.LogError("No objects tagged 'SpawnPoint' found, spawning at prefab position.");
            return prefab;
        }

        return points[_nextSpawn++ % points.Length].transform;
    }
}