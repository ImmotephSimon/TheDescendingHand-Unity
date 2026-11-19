using System;
using UnityEngine;

public interface IAnimationHandler // what Entity needs
{
    void SetAnimationState(CharacterAnimationState state);
}

public interface IAttackAnimator : IAnimationHandler
{
    void PlayAttackAnimation(AttackAnimation attack, float duration);
    void StopCurrentAnimation();
}

public interface IMovementAnimator
{
    void SetSpeed(float speed);
}

public interface IPlayerAnimationHandler : IAttackAnimator, IMovementAnimator
{
    Action PlayAnimation(AnimationClip clip, float duration);
    float PlayDodgeRoll(float duration);
    void ResetVisualRotation();
    void UpdateMovementAnimation(Vector2 input, float currentVelocity, float deltaTime, bool isDodging);
}

public interface IEnemyAnimationHandler : IAttackAnimator, IMovementAnimator
{
}