using System;
using UnityEngine;

/// <summary>
/// A stable, serializable reference to a Sprite. The Inspector drawer lets you
/// pick the Sprite itself; only its asset GUID + local file ID are serialized.
/// </summary>
[Serializable]
public struct SpriteString : IEquatable<SpriteString>
{
    [SerializeField] private string _id;

    public string Id => _id;
    public bool IsEmpty => string.IsNullOrEmpty(_id);

    public SpriteString(string id) => _id = id;

    public override string ToString() => _id ?? string.Empty;

    public bool Equals(SpriteString other) =>
        string.Equals(_id, other._id, StringComparison.Ordinal);

    public override bool Equals(object obj) =>
        obj is SpriteString other && Equals(other);

    public override int GetHashCode() =>
        _id == null ? 0 : StringComparer.Ordinal.GetHashCode(_id);

    public static bool operator ==(SpriteString left, SpriteString right) => left.Equals(right);
    public static bool operator !=(SpriteString left, SpriteString right) => !left.Equals(right);
}
