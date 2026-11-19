using FishNet;
using FishNet.Component.Observing;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonManager : MonoBehaviour, IDungeonHandler
{
    [SerializeField] private DungeonGenerator dungeonGeneratorPrefab;
    [SerializeField] private Stairs stairsPrefab;
    private Light _directionalLight;
    private Transform _stairsAnchor;
    private readonly Dictionary<Transform, DungeonGenerator> dungeonCache = new();
    private bool isGenerating;

    public int ZoneLevel => activeDungeon != null ? activeDungeon.ZoneLevel : 0;
    public static DungeonManager Instance { get; private set; }

    private DropsComponent _drops;

    public bool IsOverworld => dungeonStack.Count == 0;

    public Action<int> OnZoneLevelChanged { get; internal set; }

    private Stack<DungeonGenerator> dungeonStack = new();
    private DungeonGenerator activeDungeon;


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
        _stairsAnchor = OverworldAnchor.Instance.StairsAnchor;

        Debug.Assert(_directionalLight != null, "DungeonManager: no sun found. Set Sun Source in Lighting settings.", this);
        Debug.Assert(_stairsAnchor != null, "DungeonManager: Crypt/StairsAnchor not found under Overworld.", this);

        ServerRoot.Instance.Ready += InitializeOverworld;

        HingeController.OnPropSpawned += RegisterDungeonProp;

    }

    private void InitializeOverworld()
    {
        var nob = OverworldAnchor.Instance.GetComponent<NetworkObject>();
        MatchCondition.AddToMatch(0, nob);
        SpawnStairs(_stairsAnchor, 0, 1);
    }


    private void OnEnable() { Stairs.OnDescend += OnDescend; Stairs.OnAscend += OnAscend; }
    private void OnDisable() { Stairs.OnDescend -= OnDescend; Stairs.OnAscend -= OnAscend; }
    private void OnDescend(Stairs s, NetworkObject player) => EnterDungeon(s.DungeonAnchor, player);
    private void OnAscend(Stairs s, NetworkObject player) => LeaveDungeon(player);


    public void EnterDungeon(Transform anchor, NetworkObject player)
    {
        if (isGenerating || anchor == null) return;


        if (dungeonCache.TryGetValue(anchor, out var cachedDungeon))
        {
            dungeonStack.Push(cachedDungeon);
            AssignDungeon(player);
            return;
        }

        Vector3 spawnPosition = new Vector3(
            Mathf.Round(anchor.position.x / 3f) * 3f,
            anchor.position.y + 3f,
            Mathf.Round(anchor.position.z / 3f) * 3f
        );

        DungeonGenerator newDungeon = Instantiate(
            dungeonGeneratorPrefab,
            spawnPosition,
            anchor.rotation
        );
        InstanceFinder.ServerManager.Spawn(newDungeon.gameObject);

        newDungeon.ZoneLevel = activeDungeon != null
            ? activeDungeon.ZoneLevel + 1
            : 1;

        dungeonCache[anchor] = newDungeon;
        dungeonStack.Push(newDungeon);

        AssignDungeon(player);
        StartCoroutine(GenerateRoutine(newDungeon));
    }

    public void LeaveDungeon(NetworkObject player)
    {
        if (isGenerating || IsOverworld) return;

        dungeonStack.Pop();
        AssignDungeon(player);
    }

    private void AssignDungeon(NetworkObject player)
    {
        if (_directionalLight != null) _directionalLight.enabled = IsOverworld;
        SetActiveDungeonFor(player, activeDungeon);

        OnZoneLevelChanged?.Invoke(ZoneLevel);
    }

    private IEnumerator GenerateRoutine(DungeonGenerator dungeon)
    {
        isGenerating = true;
        yield return null;
        dungeon.StartGenerating(dungeonStack.Count, dungeon.ZoneLevel);
        isGenerating = false;
    }

    public void OnDungeonCompleted(Transform stairsAnchor)
    {
        int currentMatch = activeDungeon.GetInstanceID();
        int parentMatch = dungeonStack.Count > 1 ? dungeonStack.ElementAt(1).GetInstanceID() : 0;

        SpawnStairs(stairsAnchor, currentMatch, parentMatch);
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

    private void SetActiveDungeonFor(NetworkObject player, DungeonGenerator dungeon)
    {
        var matchId = dungeonStack.Count;
        Debug.LogWarning($"matchId now: {matchId}");
        //MatchCondition.AddToMatch(matchId, player, null, true);
        MatchCondition.AddToMatch(matchId, player.Owner, null, true);
        player.GetComponent<PlayerNetworkActions>()._currentMatchId.Value = matchId;
    }

    public NetworkObject SpawnStairs(Transform stairsAnchor, int matchA, int matchB)
    {
        if (!InstanceFinder.IsServerStarted) return null;

        Stairs stairs = Instantiate(stairsPrefab, stairsAnchor.position, stairsAnchor.rotation);
        var nob = stairs.GetComponent<NetworkObject>();

        MatchCondition.AddToMatch(matchA, nob, null, false);
        MatchCondition.AddToMatch(matchB, nob, null, false);

        InstanceFinder.ServerManager.Spawn(stairs.gameObject);
        return nob;
    }
}