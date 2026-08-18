using System;
using UnityEngine;

public class ClientBridge : MonoBehaviour
{
    [SerializeField] private CardRegistry cardRegistry;
    public static ClientBridge Instance { get; private set; }
    public VFXView VFXView { get; private set; }
    public GlobePositioner GlobePositioner { get; private set; }
    public IAbilitySystem AbilitySystem { get; private set; }
    public IPlayerMovement Movement { get; private set; }
    public Player Player { get; private set; }
    public PlayerHUD PlayerHUD { get; private set; }
    public CardHandController CardHandController { get; private set; }
    public PlayerNetworkComponent PlayerNetwork { get; private set; }
    public EquipmentVisuals EquipmentVisuals { get; private set; }
    public CardRegistry CardRegistry => cardRegistry;

    public event Action<IEntity> OnPlayerReady;

    private void Awake()
    {
        Instance = this;
        VFXView = GetComponentInChildren<VFXView>();
        GlobePositioner = Camera.main.GetComponentInChildren<GlobePositioner>();

        if (GlobePositioner == null)
            Debug.LogError("Failed to find GlobePositioner");
    }

    public void RegisterLocalPlayer(Player player)
    {
        Player = player;

        AbilitySystem = player.GetComponentInChildren<IAbilitySystem>();
        Movement = player.GetComponent<IPlayerMovement>();
        PlayerHUD = player.GetComponent<PlayerHUD>();
        GlobePositioner.Initialize(player);
        EquipmentVisuals = player.GetComponentInChildren<EquipmentVisuals>();
        CardHandController = Camera.main.GetComponentInChildren<CardHandController>();
        PlayerNetwork = player.GetComponent<PlayerNetworkComponent>();

        Debug.Assert(AbilitySystem != null, "Failed to register AbilitySystem");
        Debug.Assert(Movement != null, "Failed to register PlayerMovement");
        Debug.Assert(PlayerHUD != null, "Failed to register HUD");
        Debug.Assert(EquipmentVisuals != null, "Failed to register EquipmentVisuals");
        Debug.Assert(CardHandController != null, "Failed to register CardHandController");
        Debug.Assert(PlayerNetwork != null, "Failed to register PlayerNetwork");

        player.InitializeLocalPlayer();
        OnPlayerReady?.Invoke(player);
    }
}