using System;
using System.Collections.Generic;

public class ItemInstance
{
    public ItemDefinition BaseType { get; }
    public Rarity Rarity { get; }
    public List<AffixInstance> Affixes { get; }
    public Dictionary<AffixInstance, ModifierHandle> ActiveModifiers { get; } = new();
    public List<ItemUseComponent> Components { get; } = new();

    public ItemInstance(ItemDefinition baseType, Rarity rarity, List<AffixInstance> affixes)
    {
        BaseType = baseType;
        Rarity = rarity;
        Affixes = affixes ?? new List<AffixInstance>();

        foreach (var def in BaseType.Components)
        {
            Components.Add(def.CreateRuntimeComponent());
        }
    }

    public void ApplyAffixes(IEntity owner)
    {
        for (int i = 0; i < Affixes.Count; i++)
        {
            var affix = Affixes[i];
            if (ActiveModifiers.ContainsKey(affix)) continue;

            var handle = owner.Stats.AddModifier(affix.ToStatModifier());
            ActiveModifiers.Add(affix, handle);
        }
    }

    public void ClearAffixes(IEntity owner)
    {
        foreach (var handle in ActiveModifiers.Values)
        {
            owner.Stats.RemoveModifier(handle);
        }

        ActiveModifiers.Clear();
    }
}