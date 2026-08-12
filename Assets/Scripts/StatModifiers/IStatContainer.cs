using System;

public interface IStatContainer
{
    float GetStat(GameTag stat, TagContainer context);
    float GetStat(GameTag stat, TagContainer context, float baseValue);
    float GetStat(GameTag stat);
    ModifierHandle AddModifier(StatModifier modifier);
    void RemoveModifier(ModifierHandle handle);
    void Listen(GameTag stat, Action<float> listener);
    void StopListening(GameTag stat, Action<float> listener);

}