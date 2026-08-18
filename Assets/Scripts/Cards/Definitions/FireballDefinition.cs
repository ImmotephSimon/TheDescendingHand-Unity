using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Fireball")]
public class FireballDefinition : CardDefinition
{
    [SerializeField] private ProjectileInfo projectileInfo;
    [SerializeField] private float effectiveness;
    [SerializeField] private TagRestriction damageConversion;
    [SerializeField] private float duration;
    [SerializeField] private float radius;

    public override Card Create(CardInitContext context)
    {
        var card = new Card(context.InstanceId, this, context.Owner);
        var projectile = new ProjectileComponent(projectileInfo, context.ServerSpawn);
        var areaDegen = new AreaDegenComponent(
            radius,
            damageConversion,
            effectiveness,
            duration,
            1);

        projectile.OnSpawned += controller =>
        {
            areaDegen.TrackTransform(controller.transform);
            controller.OnDespawn += () => areaDegen.Stop();
        };
        
        card.AddComponent(projectile);
        card.AddComponent(areaDegen);

        return card;
    }
}