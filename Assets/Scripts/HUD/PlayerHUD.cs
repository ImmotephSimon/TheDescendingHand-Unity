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
    

    public HUD HUD { get; private set; }
    public CursorItem CursorItem { get; private set; }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        Debug.Assert(hudPrefab != null, $"{nameof(hudPrefab)} is not assigned.", this);
    }
#endif

    internal void Bind(Player player)
    {
        var instance = Instantiate(hudPrefab);
        HUD = instance.GetComponent<HUD>();
        _inventoryView = HUD.GetComponentInChildren<InventoryView>();
        _loadoutView = HUD.GetComponentInChildren<LoadoutView>();

        HUD.Bind(player.GetComponent<LevelComponent>());
        HUD.Bind(player.GetComponent<IHealth>());
        HUD.Bind(player.GetComponent<IMana>());
        _inventoryView.Bind(player);
        _loadoutView.Bind(player.GetComponent<PlayerInventory>().Loadout);
        
    }

    public void ToggleInventory()
    {
        _inventoryView.ToggleVisibility();
    }

    public IHealthBar BindBossHealthBar(IEntity enemy)
    {
        BossHealthBar bar = HUD.GetComponentInChildren<BossHealthBar>();
        enemy.Died += UnbindBossHealthBar;

        bar.enabled = true;
        bar.Bind(enemy.Transform.GetComponent<IHealth>());
        return bar;
    }

    private void UnbindBossHealthBar(IEntity enemy)
    {
        enemy.Died -= UnbindBossHealthBar;
        BossHealthBar bar = HUD.GetComponentInChildren<BossHealthBar>();
        bar.enabled = false;
        bar.Unbind();
    }
}