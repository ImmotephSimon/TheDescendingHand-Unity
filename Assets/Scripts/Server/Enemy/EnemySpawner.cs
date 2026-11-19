using FishNet;
using FishNet.Managing.Object;
using FishNet.Object;
using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private DualPrefabObjects enemyDualPrefab;
    [SerializeField] public EnemyDefinition enemyDefinition;

    [SerializeField] private bool spawnOnEnter = true;
    [SerializeField] private int spawnCount = 3;
    [SerializeField] public Transform fixedSpawnPoint;

    private Room room;
    private Collider spawnArea;

    public event Action<ServerEnemy> OnSpawned;

    private void Awake()
    {
        room = GetComponentInParent<Room>();
        spawnArea = room.Bounds;
        // Search for the collider on any child objects (like your "Bounds" object)
        if (spawnArea == null) Debug.LogError($"[Spawner] Could not find any Collider on {gameObject.name} or its children!");
        if (room == null) Debug.LogError("EnemySpawner requires a Room parent.", this);

        room.Entered += SpawnEnemies;
    }


    private Vector3 GetRandomFloorPoint()
    {
        if (fixedSpawnPoint != null)
            return fixedSpawnPoint.position;

        Bounds bounds = spawnArea.bounds;

        Vector3 spawnPoint = new Vector3(
            UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
            room.transform.position.y + room.FloorHeight + 1.5f,
            UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
        );

        if (Physics.Raycast(
            spawnPoint,
            Vector3.down,
            out RaycastHit hit,
            room.RoomExtent.y + 2f,
            LayerMask.GetMask("Floor"),
            QueryTriggerInteraction.Ignore))
        {
            if (NavMesh.SamplePosition(
                hit.point,
                out NavMeshHit navHit,
                3f * 0.5f,
                NavMesh.AllAreas))
            {
                return navHit.position + Vector3.up * 0.1f;
            }
        }

        return Vector3.zero;
    }

    public void Spawn(Vector3 point)
    {
        if (!InstanceFinder.IsServerStarted)
            return;

        if (enemyDualPrefab == null)
        {
            Debug.LogError($"No server enemy prefab assigned on {name}.", this);
            return;
        }

        NetworkObject serverPrefab = enemyDualPrefab.Prefabs[0].Server;
        NetworkObject nob = Instantiate(serverPrefab, point, Quaternion.identity);
        if (nob == null)
        {
            Debug.LogError("Failed to instantiate enemy prefab.", this);
            return;
        }

        ServerEnemy enemy = nob.GetComponent<ServerEnemy>();
        if (enemy == null)
        {
            Debug.LogError($"{nob.name} has no Enemy component.", this);
            return;
        }

        // Initialize definition & stats BEFORE server spawn call
        enemy.Initialize(enemyDefinition, DungeonManager.Instance.ZoneLevel);

        InstanceFinder.ServerManager.Spawn(nob);
        Debug.Log($"[SERVER] enemy {nob.ObjectId} pos {nob.transform.position} | spawner room {room.name} at {room.transform.position}");

        nob.GetComponent<ISpawnable>()?.OnSpawnComplete();

        OnSpawned?.Invoke(enemy);
    }
    public void SpawnEnemies()
    {
        if (!spawnOnEnter)
            return;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 point = GetRandomFloorPoint();

            if (point != Vector3.zero)
                Spawn(point);
        }
    }
}