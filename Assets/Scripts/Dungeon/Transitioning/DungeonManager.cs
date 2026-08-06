using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] private DungeonGenerator dungeonGeneratorPrefab;
    [SerializeField] private int maxCachedFloors = 3;

    private readonly Dictionary<int, DungeonGenerator> dungeonCache = new();
    private int currentDepth = 0;

    public static DungeonManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    internal void EnterDungeon(Transform anchor)
    {
        // Hide current level before moving down
        if (dungeonCache.TryGetValue(currentDepth, out var current))
            SetDungeonVisible(current, false);

        currentDepth++;
        LoadOrGenerateFloor(anchor);
    }

    internal void LeaveDungeon(Transform anchor)
    {
        if (currentDepth <= 0) return;

        // Hide current level before moving up
        if (dungeonCache.TryGetValue(currentDepth, out var current))
            SetDungeonVisible(current, false);

        currentDepth--;

        // Unhide previous floor if returning to it
        if (dungeonCache.TryGetValue(currentDepth, out var previous))
            SetDungeonVisible(previous, true);
    }

    private void LoadOrGenerateFloor(Transform anchor)
    {
        if (dungeonCache.TryGetValue(currentDepth, out var existing))
        {
            existing.gameObject.SetActive(true);
            return;
        }

        Vector3 position = new Vector3(
            Mathf.Round(anchor.position.x / 3f) * 3f,
            anchor.position.y + 3f,
            Mathf.Round(anchor.position.z / 3f) * 3f
        );

        var newDungeon = Instantiate(dungeonGeneratorPrefab, position, anchor.rotation);
        dungeonCache[currentDepth] = newDungeon;

        StartCoroutine(GenerateNextFrame(newDungeon));
        PruneOldFloors();
    }

    private IEnumerator GenerateNextFrame(DungeonGenerator dungeon)
    {
        yield return null;
        dungeon.StartGenerating();
    }

    private void PruneOldFloors()
    {
        int oldestAllowed = currentDepth - maxCachedFloors;
        if (dungeonCache.TryGetValue(oldestAllowed, out var oldDungeon))
        {
            Destroy(oldDungeon.gameObject);
            dungeonCache.Remove(oldestAllowed);
        }
    }
    private void SetDungeonVisible(DungeonGenerator dungeon, bool visible)
    {
        //foreach (var r in dungeon.GetComponentsInChildren<Renderer>())
        //    r.enabled = visible;

        //foreach (var c in dungeon.GetComponentsInChildren<Collider>())
        //    c.enabled = visible;
        dungeon.gameObject.SetActive(visible);
    }

    internal void OnDungeonCompleted(BossLocation bossLocation)
    {
        throw new NotImplementedException();
    }
}