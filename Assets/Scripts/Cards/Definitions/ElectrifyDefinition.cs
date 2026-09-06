using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Electrocute")]
public class ElectrifyDefinition : CardDefinition
{
    [SerializeField] private ProjectileInfo projectileInfo;
    [SerializeField] private float effectiveness;
    [SerializeField] private GameTag damageConversion;

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        var damage = card.AddCardComponent<DirectDamageComponent>();
        damage.Configure(
            effectiveness,
            damageConversion);

        var projectile = card.AddCardComponent<ProjectileComponent>();
        projectile.Configure(
            projectileInfo,
            context.ServerSpawn);

        var statusEffect = card.AddCardComponent<StatusEffectComponent>();
        statusEffect.Configure(
            GameTags.StatusElectrified,
            4f);
    }
}