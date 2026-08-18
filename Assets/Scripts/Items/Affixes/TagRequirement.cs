using System;

[Serializable]
public struct TagRequirement
{
    public bool MatchAll;
    public TagContainer Tags;

    public TagRequirement(TagContainer tags, bool matchAll = false)
    {
        Tags = tags;
        MatchAll = matchAll;
    }

    public static TagRequirement Empty => new()
    {
        MatchAll = false,
        Tags = TagContainer.Empty
    };
    public readonly bool IsEmpty => Tags == null || Tags.IsEmpty;

    public readonly bool IsSatisfiedBy(TagContainer context)
    {
        if (IsEmpty) return true;
        if (context == null) return false;

        return MatchAll
            ? context.HasAll(Tags)
            : context.HasAny(Tags);
    }


    public readonly bool IsElemental =>
        !MatchAll &&
        Tags != null &&
        Tags.HasTag(GameTags.RestrictionElementFire) &&
        Tags.HasTag(GameTags.RestrictionElementCold) &&
        Tags.HasTag(GameTags.RestrictionElementLightning);


    public override string ToString()
    {
        if (IsEmpty || Tags == null)
            return string.Empty;

        return Tags.ToString();
    }
}