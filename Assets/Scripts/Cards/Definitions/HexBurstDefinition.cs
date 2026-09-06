using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Hex Burst")]
public class HexBurstDefinition : CardDefinition
{
    [SerializeField] private float radius = 3f;
    [SerializeField] private float effectiveness;
    [SerializeField] private GameTag damageConversion;

    public override void Construct(CardInitContext context, CardRuntime card)
    {

        var overlap = card.AddSphereOverlap(radius);
        var damage = card.AddCardComponent<DirectDamageComponent>();
        damage.Configure(effectiveness, damageConversion, triggerOnHit: false);


        card.OnHit += info =>
        {
            var entity = info.Target;
            foreach (GameTag hex in GameTags.Hexes)
            {
                if (entity.Stats.GetStat(hex) > 0f)
                {
                    damage.TriggerDamage(
                        info,
                        effectiveness);
                    entity.Stats.RemoveModifier(hex);
                    context.ClientSpawn.Invoke(this, new VfxSpawnParams(entity.Transform.position));
                }
            }
        };
    }
}