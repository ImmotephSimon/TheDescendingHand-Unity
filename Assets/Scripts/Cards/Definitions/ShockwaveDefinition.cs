using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Shockwave")]
public class ShockwaveDefinition : CardDefinition
{
    [SerializeField] private float radius = 8f;
    [SerializeField] private float force = 20f;
    [SerializeField] private float upwardModifier = 0f;
    [SerializeField] private float effectiveness;
    [SerializeField] private GameTag damageConversion;
    [SerializeField] private float delayDuration = 1f;

    private static int _debrisLayerMask = -1;
    private static int DebrisLayerMask => _debrisLayerMask == -1
        ? (_debrisLayerMask = LayerMask.GetMask("Debris"))
        : _debrisLayerMask;

    public override void Construct(CardInitContext context, Card card)
    {
        var overlap = new AreaOverlapComponent(radius);
        var damage = new DirectDamageComponent(effectiveness, damageConversion, triggerOnHit: false);
        var delay = new DelayedComponent(delayDuration);

        delay.OnCompleted += () =>
        {
            Vector3 epicentre = card.TargetLocation;

            context.ClientSpawn?.Invoke(this, new VfxSpawnParams
            {
                Position = epicentre,
                Scale = Vector3.one * radius
            });

            // Include both Debris AND the AttackLayer so unbroken boulders are detected
            int targetMask = DebrisLayerMask | (1 << card.Owner.AttackLayer);
            Collider[] hits = Physics.OverlapSphere(epicentre, radius, targetMask);

            foreach (var col in hits)
            {
                // 1. Break the fracture system if unbroken
                if (col.GetComponentInParent<FractureComponent>() is FractureComponent fracture)
                {
                    fracture.Break(epicentre, force, radius, upwardModifier);
                }

                if (col.TryGetComponent<DebrisCollisionNotifier>(out var collisionNotifier))
                {
                    collisionNotifier.Initialize((targetGo, hitPoint) =>
                    {
                        if (targetGo.TryGetComponent<IEntity>(out var targetEntity))
                        {
                            damage.TriggerDamage(new HitInfo(targetEntity, card.Owner, hitPoint));
                        }
                    });
                }
            }

            overlap.TriggerAt(epicentre);
        };

        card.AddComponent(overlap);
        card.AddComponent(damage);
        card.AddComponent(delay);
    }
}