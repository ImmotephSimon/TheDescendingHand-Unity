using System;

public interface IAbilitySystem
{
    void RequestCancelAbility(int index);
    void RequestUseAbility(int  cardIndex);
}