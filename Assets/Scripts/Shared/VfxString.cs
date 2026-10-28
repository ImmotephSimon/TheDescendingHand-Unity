using System;
using UnityEngine;

[Serializable]
public struct VfxString : IEquatable<VfxString>
{
    [SerializeField] private string _id;

    public string Id => _id;
    public bool IsEmpty => string.IsNullOrEmpty(_id);

    public VfxString(string id) => _id = id;

    public override string ToString() => _id ?? string.Empty;

    public bool Equals(VfxString other) =>
        string.Equals(_id, other._id, StringComparison.Ordinal);

    public override bool Equals(object obj) =>
        obj is VfxString other && Equals(other);

    public override int GetHashCode() =>
        _id == null ? 0 : StringComparer.Ordinal.GetHashCode(_id);

    public static bool operator ==(VfxString left, VfxString right) => left.Equals(right);
    public static bool operator !=(VfxString left, VfxString right) => !left.Equals(right);
}