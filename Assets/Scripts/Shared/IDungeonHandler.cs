using System;

public interface IDungeonHandler
{
    int ZoneLevel { get; }
    Action<int> OnZoneLevelChanged { get; }

    event Action<int> ZoneLevelChanged;
}