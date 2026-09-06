using System;
using UnityEngine;

public class AreaOverlapComponent : CardComponent
{
    private Collider _collider;
    private TriggerDetector _detector;

    public event Action<IEntity> OnEntityEntered;
    public event Action<IEntity> OnEntityExited;

    public void Configure(Collider collider)
    {
        _collider = collider;
        _detector = Card.gameObject.AddComponent<TriggerDetector>();
        _detector.OnEntered += entity => OnEntityEntered?.Invoke(entity);
        _detector.OnExited += entity => OnEntityExited?.Invoke(entity);

        OnEntityEntered += entity => Card.OnHit?.Invoke(new HitInfo(entity, Owner, Card.transform.position));
    }

    protected override void OnActivate()
    {
        base.OnActivate();
        _collider.enabled = true;
    }


    public void Attach(Transform targetTransform)
    {
        Card.transform.SetParent(targetTransform, worldPositionStays: false);
        Card.transform.localPosition = Vector3.zero;
        _collider.enabled = true;
    }


    protected override void OnCancel()
    {
        _collider.enabled = false;
    }
}