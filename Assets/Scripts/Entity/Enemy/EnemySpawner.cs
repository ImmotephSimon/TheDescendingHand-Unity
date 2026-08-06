using FishNet;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] public GameObject enemyPrefab;
    [SerializeField] private bool spawnOnEnter = true;
    [SerializeField] private int spawnCount = 3;
    [SerializeField] public Transform fixedSpawnPoint;
    private Room room;
    private Collider spawnArea;

    public event Action<Enemy> OnSpawned;

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
        Vector3 spawnPoint;
        if (fixedSpawnPoint != null) spawnPoint = fixedSpawnPoint.position;
        else
        {
            Bounds bounds = spawnArea.bounds;

            spawnPoint = new Vector3(
                UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                bounds.max.y + 5f,
                UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
            );
        }

        if (Physics.Raycast(
            spawnPoint,
            Vector3.down,
            out RaycastHit hit,
            20f,
            LayerMask.GetMask("Floor"),
            QueryTriggerInteraction.Ignore))
        {
            if (NavMesh.SamplePosition(
                hit.point,
                out NavMeshHit navHit,
                2f,
                NavMesh.AllAreas))
            {
                return navHit.position + Vector3.up * 0.1f;
            }
        }

        return Vector3.zero;
    }
    //public void SpawnEnemies()
    //{
    //    if (!InstanceFinder.IsServerStarted) return;
    //    if (!spawnerEnabled) return;

    //    for (int i = 0; i < spawnCount; i++)
    //    {
    //        Vector3 point = GetRandomFloorPoint();

    //        if (point == Vector3.zero)
    //        {
    //            Debug.LogWarning("[Spawner] Failed to find spawn point.");
    //            continue;
    //        }
    //        GameObject obj = Instantiate(enemyPrefab, point, Quaternion.identity);

    //        InstanceFinder.ServerManager.Spawn(obj);

    //        Debug.Log($"[Spawner] Spawned {obj.name} via FishNet", obj);

    //        ISpawnable spawnable = obj.GetComponent<ISpawnable>();

    //        spawnable.OnSpawnComplete();
    //    }
    //}
    public void Spawn(GameObject prefab, Vector3 point)
    {
        if (!InstanceFinder.IsServerStarted)
            return;

        GameObject obj = Instantiate(prefab, point, Quaternion.identity);

        InstanceFinder.ServerManager.Spawn(obj);

        ISpawnable spawnable = obj.GetComponent<ISpawnable>();
        spawnable?.OnSpawnComplete();
        Enemy enemy = obj.GetComponent<Enemy>();

        if (enemy == null)
        {
            Debug.LogError($"{obj.name} has no Enemy component.");
            return;
        }

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
                Spawn(enemyPrefab, point);
        }
    }
}