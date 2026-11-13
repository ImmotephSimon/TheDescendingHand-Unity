using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour, IDungeonHandler
{
    [SerializeField] private DungeonGenerator dungeonGeneratorPrefab;
    [SerializeField] private DungeonDecorator overworldDecorator;
    private Light _directionalLight;
    private GameObject _overworld;
    private Transform _stairsAnchor;
    private readonly Dictionary<Transform, DungeonGenerator> dungeonCache = new();
    private DungeonGenerator activeDungeon;
    private bool isGenerating;

    public int ZoneLevel => activeDungeon != null ? activeDungeon.ZoneLevel : 0;
    public static DungeonManager Instance { get; private set; }

    private DropsComponent _drops;


    public DungeonGenerator ActiveDungeon => IsOverworld ? null : dungeonStack.Peek();
    public bool IsOverworld => dungeonStack.Count == 0;

    public Action<int> OnZoneLevelChanged { get; internal set; }

    private Stack<DungeonGenerator> dungeonStack = new();


    public event Action<int> ZoneLevelChanged
    {
        add => OnZoneLevelChanged += value;
        remove => OnZoneLevelChanged -= value;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _drops = GetComponent<DropsComponent>();
        _directionalLight = RenderSettings.sun;
        _overworld = GameObject.FindWithTag("Overworld");
        _stairsAnchor = _overworld.transform.Find("Crypt/StairsAnchor");

        Debug.Assert(_directionalLight != null, "DungeonManager: no sun found. Set Sun Source in Lighting settings.", this);
        Debug.Assert(_overworld != null, "DungeonManager: no object tagged 'Overworld' found.", this);
        Debug.Assert(_stairsAnchor != null, "DungeonManager: Crypt/StairsAnchor not found under Overworld.", this);

        ServerRoot.Instance.Ready += SpawnOverworldStairs;

        HingeController.OnPropSpawned += RegisterDungeonProp;

    }

    private void SpawnOverworldStairs()
    {
        overworldDecorator.SpawnStairs(_stairsAnchor);
    }

    private void OnEnable() { Stairs.OnDescend += OnDescend; Stairs.OnAscend += OnAscend; }
    private void OnDisable() { Stairs.OnDescend -= OnDescend; Stairs.OnAscend -= OnAscend; }
    private void OnDescend(Stairs s) => EnterDungeon(s.DungeonAnchor);
    private void OnAscend(Stairs s) => LeaveDungeon();

    public void EnterDungeon(Transform anchor)
    {
        if (isGenerating || anchor == null) return;

        SetCurrentVisible(false);
        _overworld.SetActive(false);

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
            _overworld.SetActive(true);
            if (_directionalLight != null) _directionalLight.enabled = true;
            OnZoneLevelChanged?.Invoke(0);
        }
        else
        {
            AssignDungeon(dungeonStack.Peek());
        }
    }

    private void AssignDungeon(DungeonGenerator dungeon)
    {
        activeDungeon = dungeon;
        if (_directionalLight != null) _directionalLight.enabled = IsOverworld;
        SetCurrentVisible(true);

        OnZoneLevelChanged?.Invoke(ZoneLevel);
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

    public void OnDungeonCompleted(Transform stairsAnchor)
    {
        activeDungeon.Decorator.SpawnStairs(stairsAnchor);
    }

    public void RegisterDungeonProp(
        HingeController hingeController,
        HingeController.PropType propType)
    {
        if (propType != HingeController.PropType.Chest)
            return;

        Action handler = null;
        handler = () =>
        {
            hingeController.PlayerInteracted -= handler;

            for (int i = 0; i < 5; i++)
            {
                _drops.DropFromChest(
                    hingeController.transform.position,
                    hingeController.transform.forward
                );
            }
        };

        hingeController.PlayerInteracted += handler;
    }

}