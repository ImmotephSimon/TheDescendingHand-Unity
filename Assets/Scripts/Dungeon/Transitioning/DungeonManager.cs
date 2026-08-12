using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] private DungeonGenerator dungeonGeneratorPrefab;
    [SerializeField] private Light directionalLight;
    [SerializeField] private GameObject overworld;

    private readonly Dictionary<Transform, DungeonGenerator> dungeonCache = new();
    private DungeonGenerator activeDungeon;
    private bool isGenerating;

    public int ZoneLevel => activeDungeon != null ? activeDungeon.ZoneLevel : 0;
    public static DungeonManager Instance { get; private set; }

    public DungeonGenerator ActiveDungeon => IsOverworld ? null : dungeonStack.Peek();
    public bool IsOverworld => dungeonStack.Count == 0;
    private Stack<DungeonGenerator> dungeonStack = new();

    private GameObject stairs;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        stairs = overworld.GetComponentInChildren<Stairs>().gameObject;
        stairs.transform.SetParent(transform);
    }

    public void EnterDungeon(Transform anchor)
    {
        if (isGenerating || anchor == null) return;

        SetCurrentVisible(false);
        overworld.SetActive(false);

        if (dungeonCache.TryGetValue(anchor, out var cachedDungeon))
        {
            dungeonStack.Push(cachedDungeon);
            AssignDungeon(cachedDungeon);
            return;
        }

        Vector3 spawnPosition = new Vector3(
            Mathf.Round(anchor.position.x / 3f) * 3f,
            anchor.position.y + 3f,
            Mathf.Round(anchor.position.z / 3f) * 3f
        );

        var newDungeon = Instantiate(dungeonGeneratorPrefab, spawnPosition, anchor.rotation);
        newDungeon.ZoneLevel = (activeDungeon != null) ? activeDungeon.ZoneLevel + 1 : 1;

        dungeonCache[anchor] = newDungeon;
        dungeonStack.Push(newDungeon);

        AssignDungeon(newDungeon);
        StartCoroutine(GenerateRoutine(newDungeon));
    }

    public void LeaveDungeon()
    {
        if (isGenerating || dungeonStack.Count == 0) return;

        SetCurrentVisible(false);
        dungeonStack.Pop();

        if (IsOverworld)
        {
            activeDungeon = null;
            overworld.SetActive(true);
            if (directionalLight != null) directionalLight.enabled = true;
        }
        else
        {
            AssignDungeon(dungeonStack.Peek());
        }
    }

    private void AssignDungeon(DungeonGenerator dungeon)
    {
        activeDungeon = dungeon;
        if (directionalLight != null) directionalLight.enabled = IsOverworld;
        SetCurrentVisible(true);
    }

    private void SetCurrentVisible(bool visible)
    {
        if (activeDungeon != null)
        {
            activeDungeon.gameObject.SetActive(visible);
        }
    }

    private IEnumerator GenerateRoutine(DungeonGenerator dungeon)
    {
        isGenerating = true;
        yield return null;
        dungeon.StartGenerating();
        isGenerating = false;
    }

    internal void OnDungeonCompleted(BossLocation bossLocation)
    {
        throw new NotImplementedException();
    }
}