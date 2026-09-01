using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Fireball")]
public class FireballDefinition : CardDefinition
{
    [SerializeField] private ProjectileInfo projectileInfo;
    [SerializeField] private float effectiveness;
    [SerializeField] private GameTag damageConversion;
    [SerializeField] private float duration;
    [SerializeField] private float radius;

    public override void Construct(CardInitContext context, Card card)
    {
        var projectile = new ProjectileComponent(projectileInfo, context.ServerSpawn);
        var areaDegen = new AreaDegenComponent(
            radius,
            damageConversion,
            effectiveness,
            duration,
            1);

        projectile.OnSpawned += controller =>
        {
            areaDegen.Overlap.ToggleTick(controller.transform);
            controller.OnDespawn += areaDegen.Cancel;
        };

        card.AddComponent(projectile);
        card.AddComponent(areaDegen);
    }
}