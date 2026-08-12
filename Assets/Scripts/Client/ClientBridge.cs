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
    public PlayerHUD HUD { get; private set; }
    public CardHandController CardHandController { get; private set; }
    public EquipmentVisuals EquipmentVisuals { get; private set; }
    public CardRegistry CardRegistry => cardRegistry;


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
        HUD = player.GetComponent<PlayerHUD>();
        GlobePositioner.Initialize(player);
        EquipmentVisuals = player.GetComponentInChildren<EquipmentVisuals>();
        if (AbilitySystem == null)
            Debug.LogError("Failed to register AbilitySystem");

        if (Movement == null)
            Debug.LogError("Failed to register PlayerMovement");

        if (EquipmentVisuals == null)
            Debug.LogError("Failed to register EquipmentVisuals");

        CardHandController = Camera.main.GetComponentInChildren<CardHandController>();


        player.InitializeLocalPlayer();

        if (AbilitySystem == null)
            Debug.LogError("Failed to register AbilitySystem");

        if (Movement == null)
            Debug.LogError("Failed to register PlayerMovement");
    }
}