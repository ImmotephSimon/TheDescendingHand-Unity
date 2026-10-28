using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class GameTag : IEquatable<GameTag>
{
    [SerializeField, FormerlySerializedAs("TagId")]
    private string _tagId = string.Empty;

    public string TagId => _tagId;

    public GameTag() : this(string.Empty) { }

    public GameTag(string id)
    {
        _tagId = id ?? string.Empty;
    }
    public const string TagIdField = nameof(_tagId);

    public static GameTag Empty => new GameTag(string.Empty);

    public bool Equals(GameTag other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(TagId, other.TagId, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as GameTag);
    }

    public override int GetHashCode()
    {
        return TagId != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(TagId) : 0;
    }

    public static bool operator ==(GameTag a, GameTag b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }

    public static bool operator !=(GameTag a, GameTag b)
    {
        return !(a == b);
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(TagId)) return string.Empty;

        int lastDot = TagId.LastIndexOf('.');
        string lastName = lastDot >= 0 ? TagId.Substring(lastDot + 1) : TagId;

        return Regex.Replace(lastName, "(\\B[A-Z])", " $1");
    }
}