using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;

public class PlayerStatsNetwork : NetworkBehaviour
{
    //public readonly SyncVar<float> Health = new();
    //public readonly SyncVar<float> MaxHealth = new();
    //public readonly SyncVar<float> Mana = new();
    //public readonly SyncVar<float> MaxMana = new();
    //public readonly SyncVar<int> Level = new();
    public readonly SyncVar<int> ZoneLevel = new();
    public readonly SyncVar<float> MoveSpeed = new(1f);
    public readonly SyncVar<float> DodgeSpeedMultiplier = new(1f);

    private IPlayerStatsServerHandler _serverHandler;
    private readonly HashSet<string> _replicatedStatuses = new();

    public HashSet<string> Statuses => _replicatedStatuses;

    public event Action<GameTag, bool> StatusChanged;
    public event Action<float, float, bool> HealthChanged;
    public event Action<float, float, bool> ManaChanged;

    public override void OnStartServer()
    {
        base.OnStartServer();

        _serverHandler = GetComponent<IPlayerStatsServerHandler>();

        if (_serverHandler == null)
        {
            throw new InvalidOperationException(
                $"{nameof(IPlayerStatsServerHandler)} is missing on {gameObject.name}.");
        }

        _serverHandler.Initialize(this);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }


    [TargetRpc]
    public void HealthChangedRpc(
        NetworkConnection conn,
        float current,
        float max,
        bool isInstant)
    {
        HealthChanged?.Invoke(current, max, isInstant);
    }

    [TargetRpc]
    public void ManaChangedRpc(
        NetworkConnection conn,
        float current,
        float max,
        bool isInstant)
    {
        ManaChanged?.Invoke(current, max, isInstant);
    }

    [ObserversRpc]
    public void SyncStatusObserversRpc(GameTag status, float value)
    {
        bool isActive = value > 0f;

        if (isActive)
            _replicatedStatuses.Add(status.TagId);
        else
            _replicatedStatuses.Remove(status.TagId);

        StatusChanged?.Invoke(status, isActive);
    }
}