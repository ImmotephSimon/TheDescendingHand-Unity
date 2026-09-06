using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Firewall")]
public class FirewallDefinition : CardDefinition
{
    [SerializeField] private GameObject firewallPrefab;
    [SerializeField] private float effectiveness;
    [SerializeField] private GameTag damageConversion;
    [SerializeField] private float duration = 5f;
    [SerializeField] private float size = 1f;
    [SerializeField] private Vector3 spawnOffset;

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        var spawn = card.AddCardComponent<MeshSpawnComponent>();
        spawn.Configure(
            firewallPrefab,
            null,
            spawnOffset,
            useGravity: false);

        var overlap = card.AddBoxOverlap(
            new Vector3(4f, 2f, 1f) * size/*,overrideLayer: LayerMask.NameToLayer("Wall")*/);

        var areaDegen = card.AddCardComponent<AreaDegenComponent>();
        areaDegen.Configure(
            overlap,
            damageConversion,
            effectiveness,
            duration,
            1);

        card.OnActivated += () =>
        {
            context.ClientSpawn.Invoke(this, new VfxSpawnParams(card.TargetLocation, 0, duration));
        };
    }
}