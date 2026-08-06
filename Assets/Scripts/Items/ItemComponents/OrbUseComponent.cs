public class OrbUseComponent : ItemUseComponent, IUsable
{
    public OrbComponentDefinition Definition { get; }

    public OrbUseComponent(OrbComponentDefinition definition) => Definition = definition;

    public override void Use(ItemInstance instance, IEntity user)
    {
        var targeter = user.Transform.GetComponent<ITargetable>();
        targeter?.StartTargeting(instance, Definition.TargetRequirements, targetItem =>
        {
            Definition.ApplyEffect(targetItem);
        });
    }
}