using UnityEngine;

[CreateAssetMenu(menuName = "Restrictions/TagRestriction")]
public class TagRestriction : Restriction
{
    public TagContainer Tags;

    public override bool AppliesTo(ItemDefinition item)
    {
        foreach (var tag in Tags.Tags)
        {
            if (!item.Tags.HasTag(tag))
                return false;
        }

        return true;
    }
}