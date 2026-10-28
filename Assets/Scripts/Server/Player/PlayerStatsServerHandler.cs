using FishNet.Object;

public class PlayerStatsServerHandler : NetworkBehaviour, IPlayerStatsServerHandler
{
    private IStatContainer _stats;
    private IHealth _healthHandler;
    private IMana _manaHandler;
    private PlayerStatsNetwork _network;

    public void Initialize(PlayerStatsNetwork network)
    {
        _network = network;

        _healthHandler = GetComponent<IHealth>();
        _manaHandler = GetComponent<IMana>();
        _stats = GetComponent<IStatContainer>();

        _healthHandler.OnHealthChanged += OnHealthChanged;
        _manaHandler.OnManaChanged += OnManaChanged;

        DungeonManager.Instance.ZoneLevelChanged += OnZoneLevelChanged;
        _stats.Listen(GameTags.ModStatMovement, OnMovementStatChanged);
        OnMovementStatChanged(_stats.GetStat(GameTags.ModStatMovement));

        foreach (var tag in GameTags.Statuses)
        {
            _stats.Listen(tag, _ => OnStatusChanged(tag));
        }
    }

    private void OnMovementStatChanged(float newValue)
    {
        _network.MoveSpeed.Value = newValue;

        var (_, additive, multiplicative) =
            _stats.GetScaling(GameTags.ModStatMovement);
        _network.DodgeSpeedMultiplier.Value = (1 + additive) * multiplicative;
    }

    private void OnZoneLevelChanged(int level)
    {
        _network.ZoneLevel.Value = level;
    }


    private void OnStatusChanged(GameTag tag)
    {
        _network.SyncStatusObserversRpc(tag, _stats.GetStat(tag));
    }

    private void OnHealthChanged(float current, float max, bool isInstant)
    {
        if (!IsServerInitialized)
            return;

        _network.HealthChangedRpc(Owner, current, max, isInstant);
    }

    private void OnManaChanged(float current, float max, bool isInstant)
    {
        if (!IsServerInitialized)
            return;

        _network.ManaChangedRpc(Owner, current, max, isInstant);
    }

    private void OnDestroy()
    {
        if (_healthHandler != null)
            _healthHandler.OnHealthChanged -= OnHealthChanged;

        if (_manaHandler != null)
            _manaHandler.OnManaChanged -= OnManaChanged;
    }
}