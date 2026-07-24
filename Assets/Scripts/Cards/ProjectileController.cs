using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class ProjectileController : NetworkBehaviour
{
    [SerializeField] private LayerMask _hitMask;

    public event Action<HitInfo> OnHit;

    private readonly SyncVar<AbilityVisual> _visual = new();
    private Vector3 _velocity;
    private bool _initialized;
    private SphereCollider _collider;
    private ProjectileInfo _info;
    private IEntity _owner;

    private void Awake()
    {
        _collider = GetComponentInChildren<SphereCollider>();
    }

    public void Initialize(ProjectileInfo info, IEntity owner)
    {
        _visual.Value = info.Visual;

        _info = info;
        _owner = owner;
        _initialized = true;
        _hitMask |= 1 << _owner.TargetTeamLayer; 
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (!_initialized)
            return;

        _velocity = _info.GetLaunchVelocity(_owner);
    }

    [Server]
    private void Update()
    {
        if (!_initialized) return;

        Vector3 movement = _velocity * Time.deltaTime;

        CheckCollision(transform.position, movement);

        transform.position += movement;
    }

    private void CheckCollision(Vector3 start, Vector3 movement)
    {
        if (Physics.SphereCast(
            start,
            _collider.radius,
            movement.normalized,
            out RaycastHit hit,
            movement.magnitude,
            _hitMask))
        {
            HandleHit(hit.collider);
        }
    }

    private void HandleHit(Collider collider)
    {
        if (collider.TryGetComponent<IEntity>(out var target))
        {
            HitInfo hit = new(
                target,
                _owner,
                transform.position
            );
            OnHit?.Invoke(hit);
        }

        Despawn();
    }


    public override void OnStartClient()
    {
        if (_visual.Value == AbilityVisual.None)
        {
            Debug.LogWarning($"Projectile {name} has no visual assigned.");
            return;
        }

        ClientBridge.Instance.VFXView.AttachAbilityVisual(
            _visual.Value,
            transform);
    }

    public override void OnStopClient()
    {
        ClientBridge.Instance.VFXView.DetachAbilityVisual(
            _visual.Value,
            transform);
    }
}