using System;

public interface IStatContainer
{
    float GetStat(GameTag stat, TagContainer context);
    float GetStat(GameTag stat, TagContainer context, float baseValue);
    float GetStat(GameTag stat);
    ModifierHandle AddModifier(StatModifier modifier);
    void RemoveModifier(ModifierHandle handle);

    event Action<GameTag> OnStatChanged;
}