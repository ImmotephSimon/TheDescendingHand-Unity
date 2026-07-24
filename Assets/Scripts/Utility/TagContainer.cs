using System;
using System.Collections.Generic;

[System.Serializable]
public class TagContainer
{
    public List<GameTag> Tags = new();
    public static TagContainer Empty => new TagContainer();

    public GameTag PrimaryTag => Tags.Count > 0 ? Tags[0] : null;

    public TagContainer() { }

    public TagContainer(TagContainer other)
    {
        if (other?.Tags != null)
            Tags = new List<GameTag>(other.Tags);
    }

    // Immutable combination helper
    public TagContainer With(GameTag tag)
    {
        var copy = new TagContainer(this);
        if (tag != null)
            copy.Tags.Add(tag);
        return copy;
    }

    public bool HasTag(GameTag tag)
    {
        foreach (var existing in Tags)
        {
            if (existing.TagId == tag.TagId)
                return true;

            if (existing.TagId.StartsWith(tag.TagId + "."))
                return true;
        }

        return false;
    }

    public bool HasAll(TagContainer required)
    {
        foreach (var tag in required.Tags)
        {
            if (!HasTag(tag))
                return false;
        }

        return true;
    }
}