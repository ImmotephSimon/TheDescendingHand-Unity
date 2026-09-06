using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Rockslide")]
public class RockslideDefinition : CardDefinition
{
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private float effectiveness;
    [SerializeField] private GameTag damageConversion;
    [SerializeField] private float radius = 1f;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 10f, 0f);
    [SerializeField] private float spread = 1.5f;
    [SerializeField] private int count = 6;
    [SerializeField] private float interval = 0.5f;
    [SerializeField] private float meshScale = 0.3f;

    public override void Construct(CardInitContext context, CardRuntime card)
    {

        var damage = card.AddCardComponent<DirectDamageComponent>();
        if (damage != null)
        {
            damage.Configure(effectiveness, damageConversion);
        }

        var spawner = card.AddCardComponent<MeshSpawnComponent>();
        if (spawner != null)
        {
            spawner.Configure(
                rockPrefab,
                context.ServerSpawn,
                spawnOffset,
                useGravity: true,
                scale: meshScale);

            spawner.OnSpawned += spawnedGo =>
            {
                Vector2 random = Random.insideUnitCircle * spread;

                spawnedGo.transform.position +=
                    new Vector3(random.x, 0f, random.y);

                spawnedGo.transform.rotation = Quaternion.Euler(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f));

                if (!spawnedGo.TryGetComponent<SphereCollider>(out var collider))
                {
                    collider = spawnedGo.AddComponent<SphereCollider>();
                }
                collider.isTrigger = true;
                collider.radius = radius;

                if (!spawnedGo.TryGetComponent<AreaOverlapComponent>(out var area))
                {
                    area = spawnedGo.AddComponent<AreaOverlapComponent>();
                }

                area.OnEntityEntered += target =>
                {
                    damage?.TriggerDamage(
                        new HitInfo(
                            target,
                            card.Owner,
                            target.Transform.position));
                };
            };
        }

        var sequence = card.AddCardComponent<SequenceComponent>();
        sequence.Configure(count, interval);
        sequence.OnSequence += () => { spawner.Activate(); };
    }
}