using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Curse")]
public class HexDefinition : CardDefinition
{
    [SerializeField] private float radius = 3f;
    [SerializeField] private float delay = 2f;
    [SerializeField] private float curseDuration = 5f;

    public override Card Create(CardInitContext context)
    {
        var card = new Card(
            context.InstanceId,
            this,
            context.Owner);

        var overlap = new AreaOverlapComponent(radius);
        var delayed = new DelayedComponent(delay);
        var statusEffect = new StatusEffectComponent(
            GameTags.StatusHexFrailty,
            curseDuration);

        card.OnActivated += () =>
        {
            context.ClientSpawn(this, new VfxSpawnParams(card.TargetLocation, delay) { Scale = Vector3.one * radius });
        };

        

        delayed.OnCompleted += () =>
        {
            overlap.TriggerAt(card.TargetLocation);
        };

        card.AddComponent(overlap);
        card.AddComponent(statusEffect);
        card.AddComponent(delayed);

        return card;
    }
}