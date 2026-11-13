using FishNet.Object;
using FishNet.Object.Synchronizing;

public class EnemyStatsNetwork : NetworkBehaviour
{
    public readonly SyncVar<float> Health = new();
    public readonly SyncVar<float> MaxHealth = new();
    public readonly SyncVar<float> HealthRate = new();

    public readonly SyncVar<EnemyType> EnemyType = new();
}