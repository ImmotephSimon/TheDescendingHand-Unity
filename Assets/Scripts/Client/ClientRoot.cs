using System;
using UnityEngine;

public class ClientRoot : MonoBehaviour
{

    public static ClientRoot Instance { get; private set; }
    public VFXView VFXView { get; private set; }
    public GlobePositioner GlobePositioner { get; private set; }
    public IPlayerMovement Movement { get; private set; }
    public CardNetwork CardHandler { get; private set; }
    public ClientPlayer ClientPlayer { get; private set; }
    public PlayerHUD PlayerHUD { get; private set; }
    public CardHandController CardHandController { get; private set; }
    public PlayerNetworkActions PlayerNetwork { get; private set; }
    public EquipmentVisuals EquipmentVisuals { get; private set; }
    public PlayerStatsNetwork Stats { get; private set; }
    public PlayerItemsNetwork ItemsSync { get; private set; }
    private PassiveTreeNetwork _passiveTreeComponent;

    public VfxCatalog VfxRegistry => VfxCatalog.Instance;

    public event Action<ClientPlayer> OnClientPlayerReady;

    private void Awake()
    {
        Instance = this;
        VFXView = GetComponentInChildren<VFXView>(true);
        GlobePositioner = Camera.main.GetComponentInChildren<GlobePositioner>(true);

        if (GlobePositioner == null)
            Debug.LogError("Failed to find GlobePositioner");
    }

    public void RegisterLocalPlayer(ClientPlayer client)
    {
        ClientPlayer = client;

        Movement = client.GetComponent<IPlayerMovement>();
        CardHandler = client.GetComponent<CardNetwork>();
        PlayerHUD = client.GetComponent<PlayerHUD>();
        EquipmentVisuals = client.GetComponentInChildren<EquipmentVisuals>(true);
        CardHandController = Camera.main.GetComponentInChildren<CardHandController>(true);
        PlayerNetwork = client.GetComponent<PlayerNetworkActions>();
        Stats = client.GetComponent<PlayerStatsNetwork>();
        ItemsSync = client.GetComponent<PlayerItemsNetwork>();

        Debug.Assert(Movement != null, "Failed to register PlayerMovement");
        Debug.Assert(CardHandler != null, "Failed to register AbilitySystem");
        Debug.Assert(PlayerHUD != null, "Failed to register HUD");
        Debug.Assert(EquipmentVisuals != null, "Failed to register EquipmentVisuals");
        Debug.Assert(CardHandController != null, "Failed to register CardHandController");
        Debug.Assert(PlayerNetwork != null, "Failed to register PlayerNetwork");
        Debug.Assert(Stats != null, "Failed to register Stats");
        Debug.Assert(ItemsSync != null, "Failed to register ItemsSync");

        client.InitializeLocalPlayer();
        OnClientPlayerReady?.Invoke(client);
    }

    private void OnEnable()
    {
        PlayerNetworkActions.SpeechBubbleReceived += OnSpeechBubbleReceived;
    }

    private void OnDisable()
    {
        PlayerNetworkActions.SpeechBubbleReceived -= OnSpeechBubbleReceived;
    }

    private void OnSpeechBubbleReceived(string text, float duration)
    {
        PlayerHUD.HUD.ShowSpeechBubble(text, duration);
    }

}