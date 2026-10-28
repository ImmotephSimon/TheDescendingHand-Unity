using UnityEngine;

public interface IPerception
{
    bool HasTarget { get; }
    Transform Target { get; }
}