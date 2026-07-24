using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Electrocute")]
public class ElectrocuteDefinition : CardDefinition
{
    [SerializeField] private ProjectileInfo projectile;
    [SerializeField] private float effectiveness;
    [SerializeField] private TagRestriction damageConversion;


    public override Card Create(CardInitContext context)
    {
        var card = new Card(
            context.InstanceId,
            this,
            context.Owner);
        
        card.AddComponent(
            new DirectDamageComponent(effectiveness, damageConversion));

        card.AddComponent(
            new ProjectileComponent(
                projectile,
                context.NetworkSpawn));
        return card;
    }
}