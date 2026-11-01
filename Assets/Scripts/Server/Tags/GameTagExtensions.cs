// server assembly
public static class GameTagServerExtensions
{
    public static TagRequirement ToRequirement(this GameTag tag)
        => new TagRequirement(new TagContainer(tag));
}