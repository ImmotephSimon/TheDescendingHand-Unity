using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Falling Boulder")]
public class FallingBoulderDefinition : CardDefinition
{
    [SerializeField] private GameObject boulderPrefab;
    [SerializeField] private float effectiveness;
    [SerializeField] private GameTag damageConversion;
    [SerializeField] private float radius = 3f;
    [SerializeField] private Vector3 spawnOffset = new(0f, 10f, 0f);

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        var spawn = card.AddCardComponent<MeshSpawnComponent>();
        spawn.Configure(boulderPrefab, context.ServerSpawn, spawnOffset, useGravity: true);

        var overlap = card.AddSphereOverlap(radius);
        var damage = card.AddCardComponent<DirectDamageComponent>();
        damage.Configure(effectiveness, damageConversion);

        spawn.OnSpawned += (spawnedGo) =>
        {
            // Set the attack layer directly on the spawned game object
            spawnedGo.layer = card.Owner.AttackLayer;

            overlap.Attach(spawnedGo.transform);

            // Ensure FractureComponent exists on the spawned instance
            if (!spawnedGo.TryGetComponent<FractureComponent>(out var fracture))
            {
                fracture = spawnedGo.AddComponent<FractureComponent>();
            }

        };

    }
}