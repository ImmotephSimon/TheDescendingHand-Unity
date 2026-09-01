using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Falling Boulder")]
public class FallingBoulderDefinition : CardDefinition
{
    [SerializeField] private GameObject boulderPrefab;
    [SerializeField] private float effectiveness;
    [SerializeField] private GameTag damageConversion;
    [SerializeField] private float radius = 3f;
    [SerializeField] private Vector3 spawnOffset = new(0f, 10f, 0f);

    public override void Construct(CardInitContext context, Card card)
    {
        var spawn = new MeshSpawnComponent(boulderPrefab, context.ServerSpawn, spawnOffset, useGravity: true);
        var overlap = new AreaOverlapComponent(radius);
        var damage = new DirectDamageComponent(effectiveness, damageConversion);

        spawn.OnSpawned += (spawnedGo) =>
        {
            // Set the attack layer directly on the spawned game object
            spawnedGo.layer = card.Owner.AttackLayer;

            // Ensure FractureComponent exists on the spawned instance
            if (!spawnedGo.TryGetComponent<FractureComponent>(out var fracture))
            {
                fracture = spawnedGo.AddComponent<FractureComponent>();
            }

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
        card.AddComponent(damage);
    }
}