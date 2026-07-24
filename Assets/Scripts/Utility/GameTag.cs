using System;

[Serializable]
public class GameTag
{
    public string TagId;

    public GameTag() { }

    public GameTag(string id)
    {
        TagId = id;
    }

    public override bool Equals(object obj)
    {
        return obj is GameTag other && TagId == other.TagId;
    }

    public override int GetHashCode()
    {
        return TagId?.GetHashCode() ?? 0;
    }

    public static bool operator ==(GameTag a, GameTag b)
    {
        if (ReferenceEquals(a, b))
            return true;

        if (a is null || b is null)
            return false;

        return a.TagId == b.TagId;
    }

    public static bool operator !=(GameTag a, GameTag b)
    {
        return !(a == b);
    }
}