using UnityEngine;

[CreateAssetMenu(menuName = "TagReactions/Frail")]
public class FrailtyReaction : ReactionType
{
    public override GameTag ListeningTag => GameTags.StatusHexFrailty;

    protected override ModifierHandle Apply(IEntity owner, float newValue)
    {
        return owner.Stats.AddModifier(new StatModifier(GameTags.ModDefenseDamageTaken, MathOp.Multiplicative, 1.2f));
    }
}