using System;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private Collider _hitCollider;
    public event Action<IEntity> OnHit;
    private void Awake()
    {
        _hitCollider.enabled = false;
    }

    public void Enable()
    {
        _hitCollider.enabled = true;
    }

    public void Disable()
    {
        _hitCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isEntity = other.TryGetComponent<IEntity>(out var target);

        if (isEntity)
        {
            OnHit?.Invoke(target);
        }
    }
}