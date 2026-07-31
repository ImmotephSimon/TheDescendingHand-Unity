using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class ProjectileController : NetworkBehaviour
{
    [SerializeField] private LayerMask _hitMask;
    [SerializeField] private LayerMask _targetMask;

    public event Action<HitInfo> OnHit;

    private readonly SyncVar<AbilityVisual> _visual = new();
    private Vector3 _velocity;
    private bool _initialized;
    private SphereCollider _collider;
    private ProjectileInfo _info;
    private IEntity _owner;
    private float _chainRadius = 4f;
    private IEntity _lastHitEntity;
    

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
        _hitMask |= 1 << _owner.OtherTeamLayer; 
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
        float distance = movement.magnitude;
        if (distance <= 0f) return;

        if (!Physics.SphereCast(start, _collider.radius, movement.normalized, out RaycastHit hit, distance, _hitMask))
            return;

        if (TryHit(hit.collider))
        {
            if (TryChain(hit.point)) return;
            RpcOnTerminalHit();
        }
        else
        {
            if (TryBounce(hit.point, CorrectNormal(movement, hit.normal))) return;
        }

        Despawn();
    }

    private bool TryHit(Collider collider)
    {
        if (!collider.TryGetComponent<IEntity>(out var target) || target == _lastHitEntity)
            return false;

        _lastHitEntity = target;
        Debug.Log($"PROJECTILE HIT {target.Transform.name} id={target.Transform.GetInstanceID()} frame={Time.frameCount}");
        OnHit?.Invoke(new HitInfo(target, _owner, transform.position));
        
        return true;
    }

    private static Vector3 CorrectNormal(Vector3 direction, Vector3 hitNormal)
    {
        return Vector3.Dot(direction, hitNormal) > 0 ? -hitNormal : hitNormal;
    }

    private bool TryChain(Vector3 hitPoint)
    {
        if (_info.Chains <= 0) return false;

        if (!TryFindNextTarget(hitPoint, out Vector3 nextDir))
            return false; // No target found -> don't spend chain, don't redirect

        _info.Chains--;
        transform.position = hitPoint;
        Redirect(nextDir);
        return true; // Chain succeeded -> stay alive
    }

    private bool TryBounce(Vector3 hitPoint, Vector3 hitNormal)
    {
        if (_info.Bounces <= 0) return false;

        _info.Bounces--;
        transform.position = hitPoint + hitNormal * 0.01f;

        Redirect(Vector3.Reflect(_velocity, hitNormal).normalized);
        return true;
    }

    private void Redirect(Vector3 newDirection)
    {
        _velocity = newDirection * _velocity.magnitude;
        transform.rotation = Quaternion.LookRotation(newDirection);

        RpcOnRedirect(transform.position, transform.rotation);
    }
    private bool TryFindNextTarget(Vector3 origin, out Vector3 direction)
    {
        direction = Vector3.zero;
        Collider[] hits = Physics.OverlapSphere(origin, _chainRadius, _targetMask);

        Collider nearest = null;
        float minDist = float.MaxValue;

        foreach (var col in hits)
        {
            if (!col.TryGetComponent<IEntity>(out var entity)) continue;
            if (entity == _lastHitEntity) continue;
            if (entity.TeamLayer == _owner.TeamLayer) continue;

            Debug.Log($"col: {col.gameObject.GetInstanceID()}");
            float dist = Vector3.SqrMagnitude(col.bounds.center - origin);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = col;
            }
        }

        if (nearest == null) return false;

        direction = (nearest.bounds.center - origin).normalized;
        return true;
    }


    [ObserversRpc]
    private void RpcOnRedirect(Vector3 point, Quaternion rotation)
    {
        ClientBridge.Instance.VFXView.DetachAbilityVisual(_visual.Value, transform);
        transform.position = point;
        transform.rotation = rotation;
        ClientBridge.Instance.VFXView.AttachAbilityVisual(_visual.Value, transform);
    }

    [ObserversRpc]
    private void RpcOnTerminalHit()
    {
        ClientBridge.Instance.VFXView.DetachAbilityVisual(_visual.Value, transform);
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