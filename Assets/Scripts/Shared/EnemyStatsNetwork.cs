using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;

public class EnemyStatsNetwork : NetworkBehaviour
{
    public readonly SyncVar<float> Health = new();
    public readonly SyncVar<float> MaxHealth = new();
    public readonly SyncVar<float> HealthRate = new();

    public readonly SyncVar<EnemyType> EnemyType = new();
    public readonly SyncVar<Guid> WorldModelId = new();
    public readonly SyncVar<CharacterAnimationState> AnimationState = new();

    public event Action<Guid> WorldModelChanged;
    public event Action<CharacterAnimationState> AnimationStateChanged;
    public event Action<AttackAnimation, float> AttackAnimationPlayed;

    [ObserversRpc]
    public void PlayAttackAnimation(AttackAnimation attackAnimation, float duration)
    {

        AttackAnimationPlayed?.Invoke(attackAnimation, duration);
    }

    public override void OnStartClient()
    {
        WorldModelId.OnChange += OnWorldModelIdChanged;

        AnimationState.OnChange += OnAnimationStateChanged;
        AnimationStateChanged?.Invoke(AnimationState.Value);
    }

    public override void OnStopClient()
    {
        WorldModelId.OnChange -= OnWorldModelIdChanged;
        AnimationState.OnChange -= OnAnimationStateChanged;
    }

    [Server]
    public void SetAnimationState(CharacterAnimationState state)
    {
        AnimationState.Value = state;
    }


    private void OnWorldModelIdChanged(Guid prev, Guid next, bool asServer)
    {
        WorldModelChanged?.Invoke(next);
    }

    private void OnAnimationStateChanged(CharacterAnimationState prev, CharacterAnimationState next, bool asServer)
    {
        if (!asServer)
            AnimationStateChanged?.Invoke(next);
    }
}