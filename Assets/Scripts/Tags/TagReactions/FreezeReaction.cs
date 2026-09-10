using UnityEngine;

[CreateAssetMenu(menuName = "TagReactions/Freeze")]
public class FreezeReaction : ReactionType
{
    public override GameTag ListeningTag => GameTags.StatusFreeze;

    protected override ModifierHandle Apply(IEntity owner, float newValue)
    {
        return owner.Stats.AddModifier(new StatModifier(GameTags.ModStatMovement, MathOp.Set, 0));
    }
}