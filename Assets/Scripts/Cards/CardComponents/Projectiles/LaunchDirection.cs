using System;
using UnityEngine;

[Serializable]
public class LaunchDirection
{
    public float Sideways = 0f;

    public float Upward = 0f;

    public float Forward = 1f;

    public LaunchDirection(float sideways, float upward, float forward)
    {
        Sideways = sideways;
        Upward = upward;
        Forward = forward;
    }

    public Vector3 GetDirection(IEntity owner)
    {
        return
            owner.Transform.right * Sideways +
            owner.Transform.forward * Forward +
            owner.Transform.up * Upward;
    }
}