using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropInstance : IInventoryItem
{
    public Guid Instance { get; protected set; } = Guid.NewGuid();
    public Vector2Int Size => BaseType.InventorySize;
    public Sprite Icon => BaseType.Appearance.Icon;
    public ItemDefinition BaseType { get; }
    public Rarity Rarity { get; }
    public List<AffixInstance> Explicits { get; }
    public Dictionary<AffixInstance, ModifierHandle> ActiveExplicits { get; } = new();
    public List<ModifierHandle> ActiveImplicits { get; } = new();

    public List<ItemUseComponent> Components { get; } = new();

    public Guid InventoryId => BaseType.Id;

    public ItemDropInstance(ItemDefinition baseType, Rarity rarity, List<AffixInstance> explicits)
    {
        BaseType = baseType;
        Rarity = rarity;
        Explicits = explicits ?? new List<AffixInstance>();

        foreach (var def in BaseType.Components)
        {
            Components.Add(def.CreateRuntimeComponent());
        }
    }

    public void ApplyModifiers(IEntity owner)
    {
        foreach (var affix in BaseType.Implicits)
        {
            var handle = owner.Stats.AddModifier(affix);
            ActiveImplicits.Add(handle);
        }


        foreach (var affix in Explicits)
        {
            var handle = owner.Stats.AddModifier(affix.ToStatModifier());
            ActiveExplicits.Add(affix, handle);
        }
    }

    public void ClearModifiers(IEntity owner)
    {
        foreach (var handle in ActiveImplicits)
        {
            owner.Stats.RemoveModifier(handle);
        }
        ActiveImplicits.Clear();


        foreach (var handle in ActiveExplicits.Values)
        {
            owner.Stats.RemoveModifier(handle);
        }

        ActiveExplicits.Clear();
    }

    public void Display()
    {
        throw new NotImplementedException();
    }
}