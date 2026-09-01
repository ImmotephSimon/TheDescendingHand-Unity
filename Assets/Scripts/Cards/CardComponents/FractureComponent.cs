using UnityEngine;

public class FractureComponent : MonoBehaviour
{
    private Collider _rootCollider;
    private Rigidbody _rootRb;
    private Collider[] _debrisColliders;
    private Rigidbody[] _debrisRbs;
    private PhysicsCollisionNotifier _notifier;

    // Declare without initializing here
    private int _debrisLayer;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        // Safe to call Unity C++ layer APIs here
        _debrisLayer = LayerMask.NameToLayer("Debris");

        _rootCollider = GetComponent<Collider>();
        _rootRb = GetComponent<Rigidbody>();

        Transform debrisTarget = transform.Find("Debris") ?? transform;

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

        if (!TryGetComponent(out _notifier))
        {
            _notifier = gameObject.AddComponent<PhysicsCollisionNotifier>();
        }

        //_notifier.OnCollision += HandleImpact;
    }

    private void HandleImpact(Vector3 impactPoint)
    {
        if (_notifier != null)
        {
            _notifier.OnCollision -= HandleImpact;
        }
        Break(impactPoint);
    }

    public void Break(Vector3 hitPoint, float force = 2f, float radius = 0.5f, float upwardModifier = 0f)
    {
        SetState(true);

        foreach (var rb in _debrisRbs)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddExplosionForce(force, hitPoint, radius, upwardModifier, ForceMode.Impulse);
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

        foreach (var rb in _debrisRbs)
        {
            if (rb == _rootRb) continue;
            rb.isKinematic = !isShattered;
        }
    }

    private void OnDestroy()
    {
        if (_notifier != null)
        {
            _notifier.OnCollision -= HandleImpact;
        }
    }
}