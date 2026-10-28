using System;

public interface IExperience
{
    int Level { get; }
    event Action<int, float> OnExperienceChanged;
    float Progress { get; }
}