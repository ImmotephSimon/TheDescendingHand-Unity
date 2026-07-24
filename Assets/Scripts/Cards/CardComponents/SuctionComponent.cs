using UnityEngine;

public class SuctionComponent : CardComponent
{
    private readonly float _radius;
    private readonly float _pullForce;
    private readonly LayerMask _enemyLayer;

    private bool _isSuctionActive;

    public SuctionComponent(
        float radius,
        float pullForce,
        LayerMask enemyLayer)
    {
        _radius = radius;
        _pullForce = pullForce;
        _enemyLayer = enemyLayer;
    }

    protected override void OnActivate()
    {
        _isSuctionActive = true;
    }

    protected override void OnCancel()
    {
        _isSuctionActive = false;
    }

    public override void Tick(float deltaTime)
    {
        if (!_isSuctionActive)
            return;

        Collider[] targets = Physics.OverlapSphere(
            Owner.Transform.position,
            _radius,
            _enemyLayer
        );

        foreach (var target in targets)
        {
            if (target.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 direction = Owner.Transform.position - target.transform.position;

                if (direction.sqrMagnitude > 0.01f)
                {
                    rb.AddForce(
                        direction.normalized * _pullForce,
                        ForceMode.Force
                    );
                }
            }
        }
    }

    protected override void OnBegin()
    {
    }
}