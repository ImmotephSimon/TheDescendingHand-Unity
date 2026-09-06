using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Curse")]
public class HexDefinition : CardDefinition
{
    [SerializeField] private float radius = 3f;
    [SerializeField] private float delay = 2f;
    [SerializeField] private float curseDuration = 5f;

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        var overlap = card.AddSphereOverlap(radius);

        var delayed = card.AddCardComponent<DelayedComponent>();
        delayed.Configure(delay);

        var statusEffect = card.AddCardComponent<StatusEffectComponent>();
        statusEffect.Configure(
            GameTags.StatusHexFrailty,
            curseDuration);

        card.OnActivated += () =>
        {
            context.ClientSpawn(
                this,
                new VfxSpawnParams(card.TargetLocation, 0, delay)
                {
                    Scale = Vector3.one * radius
                });
        };

        delayed.OnCompleted += () =>
        {
            //overlap.TriggerAt(card.TargetLocation);
            card.transform.position = card.TargetLocation;
        };
    }
}