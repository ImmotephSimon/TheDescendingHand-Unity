using UnityEngine;

[CreateAssetMenu(menuName = "TagReactions/TagOnHit")]
public class TagOnHitReaction : ReactionType
{
    [SerializeField] private GameTag listeningTag;
    public override GameTag ListeningTag => listeningTag;

    [SerializeField] private StatModifier onHitModifier;
    [SerializeField] private float onHitDuration;

    protected override ModifierHandle Apply(IEntity owner, float newValue)
    {
        owner.OnHitStats.Add((onHitModifier, onHitDuration));
        return null;
    }
}