using FishNet;
using FishNet.Transporting;
using System;
using UnityEngine;

public class ServerRoot : MonoBehaviour
{
    [SerializeField] private ServerPrefabRegistry prefabRegistry;
    [SerializeField] private ItemRegistry itemRegistry;
    [SerializeField] private CardRegistry cardRegistry;
    [SerializeField] private CardRegistry testCardRegistry;

    public CardRegistry CardRegistry => UseTestRegistry ? testCardRegistry : cardRegistry;

    private bool UseTestRegistry =>
        Application.isEditor
        && testCardRegistry != null
        && testCardRegistry.Cards.Count > 0;

    public static ServerRoot Instance { get; private set; }


    public EquipmentTypeRegistry EquipmentTypes { get; private set; }
    public bool IsReady { get; private set; }
    public event Action Ready;
    public ItemRegistry ItemRegistry => itemRegistry;



    private void Awake()
    {
        Instance = this;
        ServerPrefabRegistry.SetInstance(prefabRegistry);
        ConfigureServerPrefabCollection();
        DumpPrefabCollection("SERVER", prefabRegistry.ServerCollection);
        DumpPrefabCollection("CLIENT", prefabRegistry.ClientCollection);
        EquipmentTypes = GetComponent<EquipmentTypeRegistry>();
    }

    private void ConfigureServerPrefabCollection()
    {
        // DO NOT REMOVE InitializePrefabRange.
        // Replacing SpawnablePrefabs does NOT initialize the new collection.
        // FishNet only initializes the collection present during NetworkManager.Awake().
        var collection = prefabRegistry.ServerCollection;
        var nm = InstanceFinder.NetworkManager;
        nm.SpawnablePrefabs = collection;
        collection.InitializePrefabRange(0);
    }

    private void DumpPrefabCollection(string label, FishNet.Managing.Object.SinglePrefabObjects collection)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"===== {label} =====");

        for (int i = 0; i < collection.GetObjectCount(); i++)
        {
            var obj = collection.Prefabs[i];

            sb.AppendLine(obj == null
                ? $"[{i}] NULL"
                : $"[{i}] {obj.name} | ID={obj.PrefabId} | Match={obj.PrefabId == i}");
        }

        Debug.Log(sb.ToString());
    }

    private void Start()
    {
        var sm = InstanceFinder.ServerManager;
        sm.OnServerConnectionState += OnServerState;
        if (sm.Started) HandleStarted();
    }

    private void OnServerState(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
            HandleStarted();
    }

    private void HandleStarted()
    {
        if (IsReady) return;
        IsReady = true;

        Ready?.Invoke();
    }

    private void OnDestroy()
    {
        if (InstanceFinder.ServerManager != null)
            InstanceFinder.ServerManager.OnServerConnectionState -= OnServerState;
        if (Instance == this) Instance = null;
    }
}