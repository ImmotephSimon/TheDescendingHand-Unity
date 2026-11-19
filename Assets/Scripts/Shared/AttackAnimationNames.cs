using System;

public enum AttackAnimation
{
    MeleeFast,
    MeleeSlow,
    Charge,
    Channel,
    Special
}


public static class AttackAnimationNames
{
    public static string StateName(this AttackAnimation a) => a switch
    {
        AttackAnimation.MeleeFast => "FastAttack",
        AttackAnimation.MeleeSlow => "SlowAttack",
        AttackAnimation.Charge => "ChargeAttack",
        AttackAnimation.Special => "SpecialAttack",
        _ => throw new ArgumentOutOfRangeException(nameof(a))
    };
}