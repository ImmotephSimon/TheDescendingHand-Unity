using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;


public enum ChangeKind { Instant, Gradual }

public struct ResourceChange
{
    public ChangeKind Kind;
    public float RatePerSecond; // only used when Gradual

    public static ResourceChange Instant => new() { Kind = ChangeKind.Instant };
    public static ResourceChange Gradual(float rate) =>
        new() { Kind = ChangeKind.Gradual, RatePerSecond = rate };
}

public class PlayerStatsNetwork : NetworkBehaviour
{
    public readonly SyncVar<float> Health = new();
    public readonly SyncVar<float> MaxHealth = new();
    public readonly SyncVar<float> Mana = new();
    public readonly SyncVar<float> MaxMana = new();
    public readonly SyncVar<float> ManaPerSecond = new(); // per second: + regen, - degen, 0 = none
    public readonly SyncVar<float> HealthPerSecond = new(); // per second: + regen, - degen, 0 = none
    public readonly SyncVar<int> ZoneLevel = new();
    public readonly SyncVar<float> MoveSpeed = new(1f);
    public readonly SyncVar<float> DodgeSpeedMultiplier = new(1f);
    public readonly SyncVar<int> Level = new();
    public readonly SyncVar<float> LevelProgress = new();


    private IPlayerStatsServerHandler _serverHandler;
    private readonly HashSet<string> _replicatedStatuses = new();

    public HashSet<string> Statuses => _replicatedStatuses;

    public event Action<GameTag, bool> OnStatusChanged;
    //public event Action<float, float> OnHealthChanged;
    //public event Action<float, float> OnManaChanged;
    //public event Action<int, float> OnExperienceChanged;

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



    [ObserversRpc]
    public void SyncStatusObserversRpc(GameTag status, float value)
    {
        bool isActive = value > 0f;

        if (isActive)
            _replicatedStatuses.Add(status.TagId);
        else
            _replicatedStatuses.Remove(status.TagId);

        OnStatusChanged?.Invoke(status, isActive);
    }

    public void SetExperience(int level, float progress)
    {
        Level.Value = level;
        LevelProgress.Value = progress;
    }
}