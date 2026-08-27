using System;
using UnityEngine;

public interface IAnimationHandler
{
    void SetAnimationState(CharacterAnimationState state);
    void PlayAttackAnimation(AttackAnimation attackAnimation, Action onFinished);
    Action PlayAnimation(AnimationClip clip, float duration);

    void StopCurrentAnimation();
    void SetSpeed(float clientSmoothSpeed);
}