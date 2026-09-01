using FishNet.Object;
using System;
using UnityEngine;

public class ClientBridge : MonoBehaviour
{
    [SerializeField] private CardRegistry testCards;


    public static ClientBridge Instance { get; private set; }
    public VFXView VFXView { get; private set; }
    public GlobePositioner GlobePositioner { get; private set; }
    public IAbilitySystem AbilitySystem { get; private set; }
    public IPlayerMovement Movement { get; private set; }
    public ClientPlayer ClientPlayer { get; private set; }
    public PlayerHUD PlayerHUD { get; private set; }
    public CardHandController CardHandController { get; private set; }
    public PlayerNetworkActions PlayerNetwork { get; private set; }
    public EquipmentVisuals EquipmentVisuals { get; private set; }
    public PlayerStatsSync Stats { get; private set; }
    public VfxRegistry VfxRegistry => VfxRegistry.Instance;
    public CardRegistry CardRegistry => testCards.Cards.Count > 0? testCards : CardRegistry.Instance;

    public event Action<ClientPlayer> OnClientPlayerReady;

    private void Awake()
    {
        Instance = this;
        VFXView = GetComponentInChildren<VFXView>();
        GlobePositioner = Camera.main.GetComponentInChildren<GlobePositioner>();

        if (GlobePositioner == null)
            Debug.LogError("Failed to find GlobePositioner");
    }

    public void RegisterLocalPlayer(ClientPlayer client)
    {
        ClientPlayer = client;

        Movement = client.GetComponent<IPlayerMovement>();
        AbilitySystem = client.GetComponent<IAbilitySystem>();
        PlayerHUD = client.GetComponent<PlayerHUD>();
        EquipmentVisuals = client.GetComponentInChildren<EquipmentVisuals>();
        CardHandController = Camera.main.GetComponentInChildren<CardHandController>();
        PlayerNetwork = client.GetComponent<PlayerNetworkActions>();
        Stats = client.GetComponent<PlayerStatsSync>();

        Debug.Assert(AbilitySystem != null, "Failed to register AbilitySystem");
        Debug.Assert(Movement != null, "Failed to register PlayerMovement");
        Debug.Assert(PlayerHUD != null, "Failed to register HUD");
        Debug.Assert(EquipmentVisuals != null, "Failed to register EquipmentVisuals");
        Debug.Assert(CardHandController != null, "Failed to register CardHandController");
        Debug.Assert(PlayerNetwork != null, "Failed to register PlayerNetwork");

        client.InitializeLocalPlayer();
        OnClientPlayerReady?.Invoke(client);
    }
}