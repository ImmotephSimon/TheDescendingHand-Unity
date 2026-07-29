using System;
using UnityEngine;

public class ClientBridge : MonoBehaviour
{
    public static ClientBridge Instance { get; private set; }
    public VFXView VFXView { get; private set; }
    public IAbilitySystem AbilitySystem { get; private set; }
    public IPlayerMovement Movement { get; private set; }
    public Player Player { get; private set; }
    public PlayerHUD HUD { get; private set; }
    public event Action<Player> OnLocalPlayerRegistered;

    private void Awake()
    {
        Instance = this;
        VFXView = GetComponentInChildren<VFXView>();
    }

    public void RegisterLocalPlayer(Player player)
    {
        Player = player;
        
        AbilitySystem = player.GetComponentInChildren<IAbilitySystem>();
        Movement = player.GetComponent<IPlayerMovement>();

        OnLocalPlayerRegistered?.Invoke(player);
        player.InitializeLocalPlayer();

        if (AbilitySystem == null)
            Debug.LogError("Failed to register AbilitySystem");

        if (Movement == null)
            Debug.LogError("Failed to register PlayerMovement");
    }
}