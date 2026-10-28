using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Fireball")]
public class FireballDefinition : CardDefinition
{
    [SerializeField] private ProjectileInfo projectileInfo;
    [SerializeField] private float effectiveness;
    [SerializeField] private GameTag damageConversion;
    [SerializeField] private float duration;
    [SerializeField] private float radius;

    public ProjectileInfo ProjectileInfo { get { return projectileInfo; } }

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        var projectile = card.AddCardComponent<ProjectileComponent>();
        projectile.Configure(projectileInfo, context);

        var overlap = card.AddSphereOverlap(radius);

        var areaDegen = card.AddCardComponent<AreaDegenComponent>();
        areaDegen.Configure(
            overlap,
            damageConversion,
            effectiveness,
            duration,
            1);

        projectile.OnSpawned += controller =>
        {
            areaDegen.Overlap.Attach(controller.transform);
            controller.OnDespawn += areaDegen.Cancel;
        };
    }
}