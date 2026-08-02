using FishNet.Object;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class PlayerHUD : NetworkBehaviour
{
    [SerializeField] private GameObject hudPrefab;
    private InventoryView inventoryView;

    public override void OnStartClient()
    {
        if (!IsOwner)
            return;

        
        
    }

    internal void Bind(Player player)
    {
        var instance = Instantiate(hudPrefab);
        HUD hud = instance.GetComponent<HUD>();
        hud.Bind(player.GetComponent<LevelComponent>());
        hud.Bind(player.GetComponent<IHealth>());
        hud.Bind(player.GetComponent<IMana>());
        inventoryView = hud.GetComponentInChildren<InventoryView>();
        inventoryView.Bind(player);
        
    }

    public void ToggleInventory()
    {
        inventoryView.ToggleVisibility();
    }
}