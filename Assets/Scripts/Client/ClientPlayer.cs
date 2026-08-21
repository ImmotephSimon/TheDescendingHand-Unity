using FishNet.Object;
using System.Xml.Linq;
using UnityEngine;

public class ClientPlayer : MonoBehaviour
{
    private PlayerMovementController _playerMovement;
    private PlayerStatsSync _synchedStats;
    private PlayerItemsSync _synchedItems;

    public Vector3 CursorPosition => _playerMovement.CursorPosition;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovementController>();
        _synchedStats = GetComponent<PlayerStatsSync>();
        _synchedItems = GetComponent<PlayerItemsSync>();

        Debug.Assert(_playerMovement != null, $"{name} missing PlayerMovementController");
        Debug.Assert(_synchedStats != null, $"{name} missing PlayerStatsSync");
        Debug.Assert(_synchedItems != null, $"{name} missing PlayerItemsSync");
    }

    public void InitializeLocalPlayer()
    {
        ClientBridge.Instance.PlayerHUD.Bind(_synchedStats, _synchedItems);
        ClientBridge.Instance.GlobePositioner.Initialize(_synchedStats);
    }

    public void TryInteract()
    {
        GetComponent<PlayerNetworkActions>().RequestInteract();
    }
}