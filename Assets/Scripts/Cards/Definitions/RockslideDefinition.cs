using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Rockslide")]
public class RockslideDefinition : CardDefinition
{
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private float effectiveness;
    [SerializeField] private GameTag damageConversion;
    [SerializeField] private float radius = 1f;
    [SerializeField] private Vector3 spawnOffset = new(0f, 10f, 0f);
    [SerializeField] private float spread = 1.5f;
    [SerializeField] private int count = 6;
    [SerializeField] private float interval = 0.5f;
    [SerializeField] private float meshScale = 0.3f;

    public override Card Create(CardInitContext context)
    {
        var card = new Card(context.InstanceId, this, context.Owner);

        var overlap = new AreaOverlapComponent(radius);
        var damage = new DirectDamageComponent(
            effectiveness,
            damageConversion);

        var spawn = new MeshSpawnComponent(
            rockPrefab,
            context.ServerSpawn,
            spawnOffset,
            useGravity: true,
            scale: meshScale);

        spawn.OnSpawned += spawnedGo =>
        {
            Vector2 random = Random.insideUnitCircle * spread;
            spawnedGo.transform.position +=
                new Vector3(random.x, 0f, random.y);
            spawnedGo.transform.rotation = Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f));

            var notifier = spawnedGo.AddComponent<PhysicsCollisionNotifier>();

            notifier.OnCollision += impactPoint =>
            {
                overlap.TriggerAt(impactPoint);
            };
        };

        var sequence = new SequenceComponent(
            spawn,
            count,
            interval);

        card.AddComponent(spawn);
        card.AddComponent(sequence);
        card.AddComponent(overlap);
        card.AddComponent(damage);

        return card;
    }
}