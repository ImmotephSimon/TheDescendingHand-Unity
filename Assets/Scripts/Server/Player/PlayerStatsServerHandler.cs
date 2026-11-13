using UnityEngine;

public class PlayerStatsServerHandler : MonoBehaviour, IPlayerStatsServerHandler
{
    private IStatContainer _stats;
    private IExperience _experience;
    private IHealth _healthHandler;
    private IMana _manaHandler;
    private PlayerStatsNetwork _network;

    public void Initialize(PlayerStatsNetwork network)
    {
        _network = network;

        _healthHandler = GetComponent<IHealth>();
        _manaHandler = GetComponent<IMana>();
        _stats = GetComponent<IStatContainer>();
        _experience = GetComponent<IExperience>();

        _healthHandler.OnHealthChanged += OnHealthChanged;
        _manaHandler.OnManaChanged += OnManaChanged;

        _experience.OnExperienceChanged += _network.SetExperience;
        _network.SetExperience(_experience.Level, _experience.Progress);


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

    private void OnHealthChanged(float current, float max)
    {
        _network.HealthPerSecond.Value = _healthHandler.HealthPerSecond;
        _network.Health.Value = current;
        _network.MaxHealth.Value = max;
    }

    private void OnManaChanged(float current, float max, bool isInstant)
    {
        _network.ManaPerSecond.Value = _manaHandler.ManaPerSecond;
        _network.Mana.Value = current;
        _network.MaxMana.Value = max;
    }

    private void OnDestroy()
    {
        if (_healthHandler != null)
            _healthHandler.OnHealthChanged -= OnHealthChanged;

        if (_manaHandler != null)
            _manaHandler.OnManaChanged -= OnManaChanged;

        if (_experience != null && _network != null)
            _experience.OnExperienceChanged -= _network.SetExperience;
    }
}