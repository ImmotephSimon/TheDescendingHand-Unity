using System;
using UnityEngine;

public interface IAnimationHandler
{
    void SetAnimationState(CharacterAnimationState state);
    void PlayAnimation(AnimationClip animationClip, Action onFinished);
}