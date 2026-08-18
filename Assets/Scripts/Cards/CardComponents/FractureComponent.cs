using UnityEngine;

public class FractureComponent : CardComponent
{
    private GameObject _breakable;
    private Collider _rootCollider;
    private Rigidbody _rootRb;
    private Collider[] _debrisColliders;
    private Rigidbody[] _debrisRbs;
    private PhysicsCollisionNotifier _notifier;
    private readonly int _debrisLayer = LayerMask.NameToLayer("Debris");

    public void Attach(GameObject breakable)
    {
        Detach(); // Clean up previous hooks if reused

        _breakable = breakable;
        _rootCollider = _breakable.GetComponent<Collider>();
        _rootRb = _breakable.GetComponent<Rigidbody>();

        Transform debrisTarget = _breakable.transform.Find("Debris") ?? _breakable.transform;

        _debrisColliders = System.Array.FindAll(
            debrisTarget.GetComponentsInChildren<Collider>(true),
            col => col != _rootCollider
        );
        _debrisRbs = System.Array.FindAll(
            debrisTarget.GetComponentsInChildren<Rigidbody>(true),
            rb => rb != _rootRb
        );

        foreach (var col in _debrisColliders)
        {
            col.gameObject.layer = _debrisLayer;
        }

        SetState(isShattered: false);

        // Self-contained hook into collision detection
        if (!_breakable.TryGetComponent(out _notifier))
        {
            _notifier = _breakable.AddComponent<PhysicsCollisionNotifier>();
        }

        _notifier.OnCollision += HandleImpact;
    }

    private void HandleImpact(Vector3 impactPoint)
    {
        if (_notifier != null)
        {
            _notifier.OnCollision -= HandleImpact; // Fire once
        }
        Break(impactPoint);
    }

    public void Break(Vector3 hitPoint)
    {
        if (_breakable == null) return;

        SetState(true);

        foreach (var rb in _debrisRbs)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddExplosionForce(2f, hitPoint, 0.5f);
        }
    }

    public void ResetState()
    {
        SetState(isShattered: false);
    }

    private void SetState(bool isShattered)
    {
        if (_rootCollider) _rootCollider.enabled = !isShattered;
        if (_rootRb) _rootRb.isKinematic = isShattered;

        foreach (var col in _debrisColliders)
        {
            if (col == _rootCollider) continue;
            col.enabled = isShattered;
        }

        _breakable.layer = Owner.AttackLayer;

        foreach (var rb in _debrisRbs)
        {
            if (rb == _rootRb) continue;
            rb.isKinematic = !isShattered;
        }
    }

    private void Detach()
    {
        if (_notifier != null)
        {
            _notifier.OnCollision -= HandleImpact;
            _notifier = null;
        }
    }
}