using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Hex Burst")]
public class HexBurstDefinition : CardDefinition
{
    [SerializeField] private float radius = 3f;
    [SerializeField] private float effectiveness;
    [SerializeField] private GameTag damageConversion;

    public override Card Create(CardInitContext context)
    {
        var card = new Card(
            context.InstanceId,
            this,
            context.Owner);

        var overlap = new AreaOverlapComponent(radius);
        var damage = new DirectDamageComponent(effectiveness, damageConversion, triggerOnHit: false);

        card.OnActivated += () =>
        {
            overlap.TriggerAt(card.TargetLocation);
        };

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


        card.AddComponent(overlap);
        card.AddComponent(damage);

        return card;
    }
}