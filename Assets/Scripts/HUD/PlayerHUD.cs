using FishNet.Object;
using NUnit;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class PlayerHUD : NetworkBehaviour
{
    [SerializeField] private GameObject hudPrefab;
    private InventoryView _inventoryView;
    private LoadoutView _loadoutView;

    public CursorItem CursorItem { get; private set; }

    internal void Bind(Player player)
    {
        var instance = Instantiate(hudPrefab);
        HUD hud = instance.GetComponent<HUD>();
        _inventoryView = hud.GetComponentInChildren<InventoryView>();
        _loadoutView = hud.GetComponentInChildren<LoadoutView>();

        hud.Bind(player.GetComponent<LevelComponent>());
        hud.Bind(player.GetComponent<IHealth>());
        hud.Bind(player.GetComponent<IMana>());
        _inventoryView.Bind(player);
        _loadoutView.Bind(player.GetComponent<PlayerInventory>().Loadout);
        
    }

    public void ToggleInventory()
    {
        _inventoryView.ToggleVisibility();
    }
}