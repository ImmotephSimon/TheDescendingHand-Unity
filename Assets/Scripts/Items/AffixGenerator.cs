using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AffixGenerator
{
    public static List<AffixInstance> Generate(ItemDefinition item, Rarity rarity, System.Random seed, int misfortune = 1)
    {
        var results = new List<AffixInstance>();

        int targetCount = RollAffixCountForRarity(rarity, seed);
        var selectedDefinitions = RollWeightedAffixDefinitions(item, targetCount, seed);

        foreach (var def in selectedDefinitions)
        {
            var instance = CreateAffixInstanceWithRolledValue(def, seed, misfortune);

            if (RollsPositiveForMutationChance(seed))
            {
                instance = ApplyMutationOrConversion(instance, seed);
            }

            results.Add(instance);
        }

        return results;
    }

    public static List<AffixDefinition> RollWeightedAffixDefinitions(
    ItemDefinition item,
    int count,
    System.Random rng)
    {
        var results = new List<AffixDefinition>();

        // 1. Fetch EquipmentComponent to get the pool
        var equipComp = item.Components.OfType<EquipComponentDefinition>().FirstOrDefault();

        if (equipComp == null || equipComp.EquipmentType == null)
            return results;

        var pool = equipComp.EquipmentType.ModifierPool;
        if (pool == null || pool.Entries == null || pool.Entries.Count == 0)
            return results;

        // 2. Clone entries to prevent duplicate affixes on a single roll
        var available = new List<ModifierPoolEntry>(pool.Entries);

        // 3. Roll weighted entries
        for (int i = 0; i < count && available.Count > 0; i++)
        {
            float totalWeight = 0f;
            foreach (var entry in available)
                totalWeight += entry.Weight;

            if (totalWeight <= 0f) break;

            // System.Random uses NextDouble() for floating-point weights
            double roll = rng.NextDouble() * totalWeight;
            float currentWeight = 0f;

            for (int j = 0; j < available.Count; j++)
            {
                currentWeight += available[j].Weight;
                if (roll < currentWeight)
                {
                    if (available[j].Definition != null)
                    {
                        results.Add(available[j].Definition);
                    }

                    available.RemoveAt(j);
                    break;
                }
            }
        }

        return results;
    }

    private static AffixInstance CreateAffixInstanceWithRolledValue(
        AffixDefinition def,
        System.Random rng,
        int misfortune)
    {
        float effectiveRolls = 1f + misfortune;
        float u = (float)rng.NextDouble();
        float tier = 1f - Mathf.Pow(1f - u, 1f / effectiveRolls);

        return new AffixInstance
        {
            Definition = def,
            Tier = tier
        };
    }

    private static bool RollsPositiveForMutationChance(System.Random rng)
    {
        return rng.NextDouble() < 0.05;
    }

    private static AffixInstance ApplyMutationOrConversion(AffixInstance original, System.Random rng)
    {
        return original;
    }
    private static int RollAffixCountForRarity(Rarity rarity, System.Random rng)
    {
        if (rarity == null) return 0;
        return rng.Next(rarity.MinAffixes, rarity.MaxAffixes + 1);
    }

}