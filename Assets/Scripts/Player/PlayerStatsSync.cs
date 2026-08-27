using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsSync : NetworkBehaviour
{
    public readonly SyncVar<float> Health = new();
    public readonly SyncVar<float> MaxHealth = new();
    public readonly SyncVar<float> Mana = new();
    public readonly SyncVar<float> MaxMana = new();
    public readonly SyncVar<int> Level = new();
    private IStatContainer _stats;
    private IHealth _healthHandler;
    private IMana _manaHandler;
    private readonly HashSet<string> _replicatedStatuses = new();
    public event Action<GameTag, bool> StatusChanged;
    public event Action<float, float, bool> HealthChanged;
    public event Action<float, float, bool> ManaChanged;
    public HashSet<string> Statuses => _replicatedStatuses;

    public override void OnStartServer()
    {
        base.OnStartServer();

        //Bind(GameTags.ModStatLevel, v => Level.Value = Mathf.RoundToInt(v));

        _healthHandler = GetComponent<IHealth>();
        _manaHandler = GetComponent<IMana>();
        _healthHandler.OnHealthChanged += OnHealthChanged;
        _manaHandler.OnManaChanged += OnManaChanged;
        _stats = GetComponent<IStatContainer>();
        foreach (var tag in GameTags.Statuses)
        {
            _stats.Listen(tag, _ => OnStatusChanged(tag));
        }
    }

    private void OnStatusChanged(GameTag tag)
    {
        SyncStatusObserversRpc(tag, _stats.GetStat(tag));
    }

    private void OnHealthChanged(float current, float max, bool isInstant)
    {
        if (!IsServerInitialized) return;

        HealthChangedRpc(Owner, current, max, isInstant);
    }

    private void OnManaChanged(float current, float max, bool isInstant)
    {
        if (!IsServerInitialized) return;

        ManaChangedRpc(Owner, current, max, isInstant);
    }

    [TargetRpc]
    private void HealthChangedRpc(
        NetworkConnection conn,
        float current,
        float max,
        bool isInstant)
    {
        HealthChanged?.Invoke(current, max, isInstant);
    }

    [TargetRpc]
    private void ManaChangedRpc(
        NetworkConnection conn,
        float current,
        float max,
        bool isInstant)
    {
        ManaChanged?.Invoke(current, max, isInstant);
    }


    [ObserversRpc]
    private void SyncStatusObserversRpc(GameTag status, float value)
    {
        bool isActive = value > 0f;
        if (isActive)
            _replicatedStatuses.Add(status.TagId);
        else
            _replicatedStatuses.Remove(status.TagId);

        StatusChanged?.Invoke(status, isActive);
    }

}