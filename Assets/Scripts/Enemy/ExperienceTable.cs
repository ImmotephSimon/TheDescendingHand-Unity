using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class ExperienceTable : ScriptableObject
{
    public List<RarityMultiplier> entries;

    public int ScaleByRarity(int baseExperience, Rarity rarity)
    {
        var entry = entries.FirstOrDefault(x => x.rarity == rarity);

        if (entry == null)
            return baseExperience;

        return Mathf.RoundToInt(baseExperience * entry.multiplier);
    }
}

[System.Serializable]
public class RarityMultiplier
{
    public Rarity rarity;
    public float multiplier;
}