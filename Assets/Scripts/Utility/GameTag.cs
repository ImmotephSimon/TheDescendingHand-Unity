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

    public static GameTag Empty => new GameTag(string.Empty);

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

    public override string ToString()
    {
        if (string.IsNullOrEmpty(TagId)) return string.Empty;

        string[] parts = TagId.Split('.');
        string lastName = parts[parts.Length - 1];

        return System.Text.RegularExpressions.Regex.Replace(lastName, "(\\B[A-Z])", " $1");
    }
}