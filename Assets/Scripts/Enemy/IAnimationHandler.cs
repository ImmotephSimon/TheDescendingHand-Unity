using System;

public interface IAnimationHandler
{
    void SetAnimationState(CharacterAnimationState state);
    void PlayAnimation(AttackAnimation attackAnimation, Action onFinished);
    void PlayAnimation(CardCastAnimation animation);

    void StopCurrentAnimation();

    void SetSpeed(float clientSmoothSpeed);
}