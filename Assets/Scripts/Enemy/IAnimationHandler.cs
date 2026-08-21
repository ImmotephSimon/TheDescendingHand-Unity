using System;

public interface IAnimationHandler
{
    void SetAnimationState(CharacterAnimationState state);
    void PlayAttackAnimation(AttackAnimation attackAnimation, Action onFinished);
    void PlayCastAnimation(CardCastAnimation animation);

    void StopCurrentAnimation();
    void StopCastAnimation();
    void SetSpeed(float clientSmoothSpeed);
}