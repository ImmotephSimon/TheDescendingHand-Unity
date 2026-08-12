using FishNet.Object;
using FishNet.Object.Synchronizing;

public class PlayerStats : NetworkBehaviour
{
    public readonly SyncVar<float> MoveSpeed = new SyncVar<float>(4f);
    public readonly SyncVar<float> Gravity = new SyncVar<float>(-9.81f);

    [Server]
    public void SetMoveSpeed(float value) => MoveSpeed.Value = value;

    [Server]
    public void SetGravity(float value) => Gravity.Value = value;
}