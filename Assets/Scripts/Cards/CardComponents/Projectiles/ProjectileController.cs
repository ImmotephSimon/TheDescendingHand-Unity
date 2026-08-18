using FishNet.Object;
using System;
using UnityEngine;

public class ProjectileController : NetworkBehaviour
{
    private static int EnvironmentMask;
    private int DynamicHitMask => EnvironmentMask | (1 << _owner.HostileLayer);

    private GameObject _visual;
    private Vector3 _velocity;
    private bool _initialized;
    private SphereCollider _collider;
    private ProjectileInfo _info;
    private IEntity _owner;
    private float _chainRadius = 4f;
    private IEntity _lastHitEntity;
    private readonly float maxProjectileDistanceSqr = 1800f;

    public event Action<HitInfo> OnHit;
    public event Action OnDespawn;

    private void Awake()
    {
        _collider = GetComponentInChildren<SphereCollider>();

        if (EnvironmentMask == 0)
            EnvironmentMask = LayerMask.GetMask("Wall", "Default", "Floor");
    }

    public void Initialize(ProjectileInfo info, IEntity owner)
    {
        _visual = info.Visual;
        _info = info;
        _owner = owner;
        _velocity = _info.GetLaunchVelocity(_owner);
        _initialized = true;

        Debug.Assert(_visual != null, $"Projectile visual is null.");
        if (IsClientStarted)
        {
            ClientBridge.Instance.VFXView.AttachAbilityVisual(_visual, transform);
        }
    }

    [Server]
    private void Update()
    {
        if (!_initialized) return;

        if (_owner != null && Vector3.SqrMagnitude(transform.position - _owner.Transform.position) >= maxProjectileDistanceSqr)
        {
            End();
            return;
        }

        _velocity += Physics.gravity * _info.Direction.Gravity * Time.deltaTime;

        Vector3 movement = _velocity * Time.deltaTime;

        CheckCollision(transform.position, movement);

        transform.position += movement;
    }

    private void End()
    {
        OnDespawn?.Invoke();
        Despawn();
    }

    private void CheckCollision(Vector3 start, Vector3 movement)
    {
        float distance = movement.magnitude;
        if (distance <= 0f) return;

        if (!Physics.SphereCast(start, _collider.radius, movement.normalized, out RaycastHit hit, distance, DynamicHitMask))
            return;


        if (TryHit(hit.collider, out IEntity target))
        {
            if (_info.Pierce > 0)
            {
                _info.Pierce--;
                Debug.Log($"Piercing {target.Transform.name}");
                return;
            }

            if (TryChain(hit.point)) return;
            RpcOnTerminalHit();
        }
        else
        {
            if (TryBounce(hit.point, CorrectNormal(movement, hit.normal)))
            {
                return;
            }
            else
            {
                OnHit?.Invoke(new HitInfo(null, _owner, hit.point));
            }
        }

        End();
    }

    private bool TryHit(Collider collider, out IEntity target)
    {
        if (!collider.TryGetComponent<IEntity>(out target)
            || target == _owner
            || target == _lastHitEntity
            || target.IsDead)
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
            return false;

        _info.Chains--;
        transform.position = hitPoint;
        Redirect(nextDir);
        return true;
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

        int targetMask = 1 << _owner.HostileLayer;
        Collider[] hits = Physics.OverlapSphere(origin, _chainRadius, targetMask);

        Collider nearest = null;
        float minDist = float.MaxValue;

        foreach (var col in hits)
        {
            if (!col.TryGetComponent<IEntity>(out var entity)) continue;
            if (entity == _owner || entity == _lastHitEntity || entity.IsDead) continue;

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
        ClientBridge.Instance.VFXView.DetachAbilityVisual(_visual, transform);
        transform.position = point;
        transform.rotation = rotation;
        ClientBridge.Instance.VFXView.AttachAbilityVisual(_visual, transform);
    }

    [ObserversRpc]
    private void RpcOnTerminalHit()
    {
        ClientBridge.Instance.VFXView.DetachAbilityVisual(_visual, transform);
    }

    public override void OnStartClient()
    {
        if (_visual == null)
        {
            Debug.LogWarning($"Projectile {name} has no visual assigned.");
            return;
        }

        ClientBridge.Instance.VFXView.AttachAbilityVisual(_visual, transform);
    }

    public override void OnStopClient()
    {
        ClientBridge.Instance.VFXView.DetachAbilityVisual(_visual, transform);
    }
}