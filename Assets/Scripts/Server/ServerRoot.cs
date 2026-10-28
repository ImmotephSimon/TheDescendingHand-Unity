using FishNet;
using UnityEngine;

public class ServerRoot : MonoBehaviour
{
    public static ServerRoot Instance { get; private set; }

    [SerializeField] private GameObject gameNetworkPrefab;

    public EquipmentTypeRegistry EquipmentTypes { get; private set; }

    private void Awake()
    {
        Instance = this;
        EquipmentTypes = GetComponent<EquipmentTypeRegistry>();
    }


    private void Start()
    {
        InstanceFinder.ServerManager.Spawn(
            Instantiate(gameNetworkPrefab)
        );
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}