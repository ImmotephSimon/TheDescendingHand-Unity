using UnityEngine;

public struct DamageInfo
{
    public float Amount;
    public IEntity Source;
    public Vector3 HitPosition;

    public DamageInfo(
        float amount,
        IEntity source,
        Vector3 hitPosition)
    {
        Amount = amount;
        Source = source;
        HitPosition = hitPosition;
    }
}