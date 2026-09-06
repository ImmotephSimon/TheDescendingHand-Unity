using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Loadout
{
    private readonly Dictionary<EquipmentType, ItemDropInstance> equipped = new();

    public event Action OnLoadoutChanged;
    public event Action<ItemDropInstance> OnItemUnequipped;

    public Func<Vector2Int, bool> CanUnequipToDestination;
    private IEntity _owner;

    public IReadOnlyDictionary<EquipmentType, ItemDropInstance> Equipped => equipped;

    public Loadout(IEntity owner)
    {
        _owner = owner;
    }

    public ItemDropInstance GetEquipped(EquipmentType type)
    {
        equipped.TryGetValue(type, out var item);
        return item;
    }


    public bool Equip(ItemDropInstance item)
    {
        var equipDefinition = item.BaseType.Components.OfType<EquipComponentDefinition>().FirstOrDefault();
        if (equipDefinition == null)
            throw new InvalidOperationException($"Item '{item.BaseType.name}' lacks an EquipComponent.");

        if (equipDefinition.EquipmentType == null)
            throw new InvalidOperationException($"EquipComponent on '{item.BaseType.name}' has no EquipmentType assigned.");

        if (equipped.ContainsKey(equipDefinition.EquipmentType))
        {
            if (!Unequip(equipDefinition.EquipmentType))
                return false;
        }

        EquipUseComponent equipComponent = item.Components.OfType<EquipUseComponent>().FirstOrDefault();
        equipComponent?.Equip(item, _owner);

        equipped[equipDefinition.EquipmentType] = item;

        OnLoadoutChanged?.Invoke();
        return true;
    }


    public bool Unequip(EquipmentType slot)
    {
        Debug.Log($"Unequip slot={slot}, id={slot.GetInstanceID()}");

        foreach (var key in equipped.Keys)
            Debug.Log($"Key={key}, id={key.GetInstanceID()}");


        if (!equipped.TryGetValue(slot, out ItemDropInstance item)) return false;

        if (!CanUnequipToDestination(item.BaseType.InventorySize))
        {
            Debug.LogWarning($"[Unequip] Inventory full. Cannot unequip {item.BaseType.name}.");
            return false;
        }

        EquipUseComponent component = item.Components.OfType<EquipUseComponent>().FirstOrDefault();
        component?.Unequip(item, _owner);

        equipped.Remove(slot);
        OnItemUnequipped?.Invoke(item);
        OnLoadoutChanged?.Invoke();

        return true;
    }
}
