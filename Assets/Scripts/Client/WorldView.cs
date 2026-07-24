using UnityEngine;

public struct HitPresentationData
{
    public int SourceId;
    public int TargetId;
    public Vector3 Position;
    public int EffectId;
}

public struct AttackPresentationData
{
    public int EntityId;
    public int AnimationId;
}

public struct ProjectilePresentationData
{
    public int ProjectileId;
    public Vector3 Position;
    public Vector3 Direction;
}

/// <summary>
/// Handles the visual representation of the world state.
/// Manages non-combat world visuals based on server state and events.
/// Does not own world simulation or gameplay rules.
/// </summary>
public class WorldView : MonoBehaviour
{

    private VFXView _vfx;
    private AnimationPlayer _animation;

    
    private void Awake()
    {
        _vfx = GetComponent<VFXView>();
        _animation = GetComponent<AnimationPlayer>();
    }

    //public void ShowHit(HitPresentationData data)
    //    {
    //        _vfx.PlayHit(data);
    //    }

    //public void ShowAttack(AttackPresentationData data)
    //{
    //    _animation.PlayAttack(data);
    //}

    //public void ShowProjectile(ProjectilePresentationData data)
    //{
    //    _vfx.SpawnProjectile(data);
    //}
}
