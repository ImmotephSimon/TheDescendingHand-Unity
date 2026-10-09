using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Volcano")]
public class VolcanoDefinition : CardDefinition
{
    [SerializeField] private FireballDefinition fireball;
    [SerializeField] private int count = 12;
    [SerializeField] private float interval = 0.5f;
    [SerializeField] private float spread = 3f;
    [SerializeField] private float launchPitch = 75f;

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        card.OnActivated += () =>
        {
            card.gameObject.transform.position = card.TargetLocation;
            context.ClientSpawn(this, new VfxSpawnParams(card.TargetLocation));
        };

        var sequence = card.AddCardComponent<SequenceComponent>();
        sequence.Configure(count, interval);
        sequence.OnSequence += () =>
        {
            var instance = CardFactory.CreateCardInstance(fireball, card.Owner);
            var runtime = CardFactory.CreateRuntime(instance, true);
            if (runtime == null) return;

            Vector2 offset = Random.insideUnitCircle * spread;
            runtime.SetTargetLocation(card.gameObject.transform.position + new Vector3(offset.x, 0f, offset.y));
            runtime.Trigger(card.TargetLocation, Quaternion.Euler(-launchPitch, 0f, 0f));
        };
    }
}