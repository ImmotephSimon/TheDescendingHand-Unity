using System;
using UnityEngine;

public interface IAnimationHandler
{
    void SetAnimationState(CharacterAnimationState state);
    void PlayAnimation(AttackAnimation attackAnimation, Action onFinished);

    void PlayAnimation(CardCastAnimation animation);
    void SetSpeed(float clientSmoothSpeed);
}