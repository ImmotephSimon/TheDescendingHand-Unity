using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Falling Boulder")]
public class FallingBoulderDefinition : CardDefinition
{
    [SerializeField] private GameObject boulderPrefab;
    [SerializeField] private float effectiveness;
    [SerializeField] private TagRestriction damageConversion;
    [SerializeField] private float radius = 3f;
    [SerializeField] private Vector3 spawnOffset = new(0f, 10f, 0f);

    public override Card Create(CardInitContext context)
    {
        var card = new Card(context.InstanceId, this, context.Owner);

        var spawn = new MeshSpawnComponent(boulderPrefab, context.ServerSpawn, spawnOffset, useGravity: true);
        var overlap = new AreaOverlapComponent(radius);
        var fracture = new FractureComponent();
        var damage = new DirectDamageComponent(effectiveness, damageConversion);

        spawn.OnSpawned += (spawnedGo) =>
        {
            fracture.Attach(spawnedGo);

            // FractureComponent handles its own CollisionNotifier internally,
            // so we just grab it to trigger the area overlap damage on impact.
            if (spawnedGo.TryGetComponent<PhysicsCollisionNotifier>(out var notifier))
            {
                notifier.OnCollision += (impactPoint) =>
                {
                    overlap.TriggerAt(impactPoint);
                };
            }
        };

        card.AddComponent(spawn);
        card.AddComponent(overlap);
        card.AddComponent(fracture);
        card.AddComponent(damage);

        return card;
    }
}