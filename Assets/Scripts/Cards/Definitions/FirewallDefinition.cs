using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Firewall")]
public class FirewallDefinition : CardDefinition
{
    [SerializeField] private GameObject firewallPrefab;
    [SerializeField] private float effectiveness;
    [SerializeField] private GameTag damageConversion;
    [SerializeField] private float duration = 5f;
    [SerializeField] private float radius = 2f;
    [SerializeField] private Vector3 spawnOffset;

    public override void Construct(CardInitContext context, Card card)
    {

        var spawn = new MeshSpawnComponent(
            firewallPrefab,
            context.ServerSpawn,
            spawnOffset,
            useGravity: false);

        var areaDegen = new AreaDegenComponent(
            radius,
            damageConversion,
            effectiveness,
            duration,
            1);

        areaDegen.Overlap.ToggleTick(card.TargetLocation);

        card.AddComponent(spawn);
        card.AddComponent(areaDegen);
    }
}