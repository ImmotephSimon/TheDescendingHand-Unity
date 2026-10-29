using FishNet.Serializing;
using System;
using UnityEngine;

[Serializable]
public struct AnimationString : IEquatable<AnimationString>
{
    [SerializeField] private string _id;

    public string Id => _id;
    public bool IsEmpty => string.IsNullOrEmpty(_id);

    public AnimationString(string id) => _id = id;

    public override string ToString() => _id ?? string.Empty;

    public bool Equals(AnimationString other) =>
        string.Equals(_id, other._id, StringComparison.Ordinal);

    public override bool Equals(object obj) =>
        obj is AnimationString other && Equals(other);

    public override int GetHashCode() =>
        _id == null ? 0 : StringComparer.Ordinal.GetHashCode(_id);

    public static bool operator ==(AnimationString left, AnimationString right) => left.Equals(right);
    public static bool operator !=(AnimationString left, AnimationString right) => !left.Equals(right);
}

public static class AnimationStringSerializer
{
    public static void WriteAnimationString(this Writer writer, AnimationString value)
        => writer.WriteString(value.Id);

    public static AnimationString ReadAnimationString(this Reader reader)
        => new AnimationString(reader.ReadStringAllocated());
}