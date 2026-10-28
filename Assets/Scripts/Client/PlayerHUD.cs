using FishNet.Object;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private GameObject hudPrefab;
    private InventoryView _inventoryView;
    private LoadoutView _loadoutView;
    private PassiveTreeView _passiveTreeView;
    private BossHealthBar _bossBar;
    private PlayerNetworkActions _actions;
    private NetworkObject _currentBoss;

    public HUD HUD { get; private set; }

    private void Awake()
    {
        Debug.Assert(hudPrefab != null, $"{nameof(hudPrefab)} is not assigned.", this);
    }

    internal void Bind(PlayerStatsNetwork synchedStats, PlayerItemsNetwork synchedItems, PlayerNetworkActions actions)
    {
        var instance = Instantiate(hudPrefab);
        HUD = instance.GetComponent<HUD>();
        _inventoryView = HUD.GetComponentInChildren<InventoryView>(true);
        _loadoutView = HUD.GetComponentInChildren<LoadoutView>(true);
        _passiveTreeView = HUD.GetComponentInChildren<PassiveTreeView>(true);

        _inventoryView.Bind(synchedItems);
        _loadoutView.Bind(synchedItems);

        _bossBar = HUD.GetComponentInChildren<BossHealthBar>(true);

        _bossBar = HUD.GetComponentInChildren<BossHealthBar>(true);
        _actions = actions;
        _actions.BossBarChanged += OnBossBarChanged;

        ClientRoot.Instance.OnClientPlayerReady += OnClientPlayerReady;
    }

    private void OnClientPlayerReady(ClientPlayer _)
    {
        ClientRoot.Instance.OnClientPlayerReady -= OnClientPlayerReady;
        _inventoryView.Initialize();
    }

    public void ToggleInventory()
    {
        _inventoryView.ToggleVisibility();
    }

    public void TogglePassiveTree()
    {
        _passiveTreeView.SetVisible(!_passiveTreeView.IsVisible);
    }

    private void OnBossBarChanged(NetworkObject boss, bool show)
    {
        if (show && boss != null)
        {
            _currentBoss = boss;
            _bossBar.gameObject.SetActive(true);
            _bossBar.Bind(boss.GetComponent<IHealth>());
        }
        else if (!show && boss == _currentBoss)
        {
            _currentBoss = null;
            _bossBar.Unbind();
            _bossBar.gameObject.SetActive(false);
        }
    }
}