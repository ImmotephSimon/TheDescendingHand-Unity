using FishNet.Object;
using NUnit;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private GameObject hudPrefab;
    private InventoryView _inventoryView;
    private LoadoutView _loadoutView;
    

    public HUD HUD { get; private set; }
    public CursorItem CursorItem { get; private set; }

    private void Awake()
    {
        Debug.Assert(hudPrefab != null, $"{nameof(hudPrefab)} is not assigned.", this);
    }

    internal void Bind(PlayerStatsSync synchedStats, PlayerItemsSync synchedItems)
    {
        var instance = Instantiate(hudPrefab);
        HUD = instance.GetComponent<HUD>();
        _inventoryView = HUD.GetComponentInChildren<InventoryView>();
        _loadoutView = HUD.GetComponentInChildren<LoadoutView>();

        HUD.Bind(synchedStats.GetComponent<LevelComponent>());
        HUD.Bind(synchedStats.GetComponent<IHealth>());
        HUD.Bind(synchedStats.GetComponent<IMana>());
        _inventoryView.Bind(synchedItems);
        _loadoutView.Bind(synchedItems);

        ClientBridge.Instance.OnClientPlayerReady += OnClientPlayerReady;
    }

    private void OnClientPlayerReady(ClientPlayer _)
    {
        ClientBridge.Instance.OnClientPlayerReady -= OnClientPlayerReady;
        _inventoryView.Initialize();
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