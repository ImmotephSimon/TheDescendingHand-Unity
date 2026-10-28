using UnityEngine;

public class ClientPlayer : MonoBehaviour
{
    private PlayerMovementController _playerMovement;
    private PlayerStatsNetwork _synchedStats;
    private PlayerItemsNetwork _synchedItems;
    private PlayerNetworkActions _networkActions;

    public Vector3 CursorPosition => _playerMovement.CursorPosition;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovementController>();
        _synchedStats = GetComponent<PlayerStatsNetwork>();
        _synchedItems = GetComponent<PlayerItemsNetwork>();
        _networkActions = GetComponent<PlayerNetworkActions>();

        Debug.Assert(_playerMovement != null, $"{name} missing PlayerMovementController");
        Debug.Assert(_synchedStats != null, $"{name} missing PlayerStatsSync");
        Debug.Assert(_synchedItems != null, $"{name} missing PlayerItemsSync");
    }

    public void InitializeLocalPlayer()
    {
        ClientRoot.Instance.PlayerHUD.Bind(_synchedStats, _synchedItems, _networkActions);
        ClientRoot.Instance.GlobePositioner.Initialize(_synchedStats);
    }

    public void TryInteract()
    {
        _networkActions.RequestInteract();
    }
}