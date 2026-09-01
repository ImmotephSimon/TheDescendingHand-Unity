using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Electrocute")]
public class ElectrocuteDefinition : CardDefinition
{
    [SerializeField] private ProjectileInfo projectileInfo;
    [SerializeField] private float effectiveness;
    [SerializeField] private GameTag damageConversion;

    public override void Construct(CardInitContext context, Card card)
    {
        
        var damage = new DirectDamageComponent(
            effectiveness,
            damageConversion);

        var projectile = new ProjectileComponent(
            projectileInfo,
            context.ServerSpawn);

        var statusEffect = new StatusEffectComponent(GameTags.StatusElectrified, 4f);

        card.AddComponent(damage);
        card.AddComponent(projectile);
        card.AddComponent(statusEffect);
    }
}