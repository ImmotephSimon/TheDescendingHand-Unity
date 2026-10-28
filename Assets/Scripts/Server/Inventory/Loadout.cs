using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Loadout
{
    private readonly Dictionary<EquipmentSlot, ItemDropInstance> equipped = new();

    public event Action OnLoadoutChanged;
    public event Action<ItemDropInstance> OnItemUnequipped;

    public Func<Vector2Int, bool> CanUnequipToDestination;
    private IEntity _owner;

    public IReadOnlyDictionary<EquipmentSlot, ItemDropInstance> Equipped => equipped;

    public Loadout(IEntity owner)
    {
        _owner = owner;
    }

    public ItemDropInstance GetEquipped(EquipmentSlot slot)
    {
        equipped.TryGetValue(slot, out var item);
        return item;
    }

    public bool Equip(ItemDropInstance item)
    {
        var equipDefinition = item.BaseType.Components
            .OfType<EquipComponentDefinition>()
            .FirstOrDefault();

        if (equipDefinition == null)
            throw new InvalidOperationException(
                $"Item '{item.BaseType.name}' lacks an EquipComponent.");

        EquipmentSlot slot = equipDefinition.Slot;

        if (equipped.ContainsKey(slot) && !Unequip(slot))
            return false;

        var equipComponent = item.Components
            .OfType<EquipUseComponent>()
            .FirstOrDefault();

        equipComponent?.Equip(item, _owner);

        equipped[slot] = item;

        OnLoadoutChanged?.Invoke();
        return true;
    }

    public bool Unequip(EquipmentSlot slot)
    {
        if (!equipped.TryGetValue(slot, out var item))
            return false;

        if (!CanUnequipToDestination(item.BaseType.InventorySize))
        {
            Debug.LogWarning(
                $"[Unequip] Inventory full. Cannot unequip {item.BaseType.name}.");
            return false;
        }

        var component = item.Components
            .OfType<EquipUseComponent>()
            .FirstOrDefault();

        component?.Unequip(item, _owner);

        equipped.Remove(slot);
        OnItemUnequipped?.Invoke(item);
        OnLoadoutChanged?.Invoke();

        return true;
    }
}