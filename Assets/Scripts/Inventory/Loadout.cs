using System.Collections.Generic;
using Unity.VisualScripting;

public class Loadout
{
    private readonly Dictionary<EquipmentType, ItemInstance> equipped = new();


    public bool Equip(ItemInstance item)
    {
        var component = item.BaseType.GetComponent<EquipComponent>();

        if (component == null)
            return false;

        if (equipped.ContainsKey(component.EquipmentType))
            Unequip(component.EquipmentType);

        equipped[component.EquipmentType] = item;

        component.Equip();

        return true;
    }

    public bool Unequip(EquipmentType slot)
    {
        if (!equipped.TryGetValue(slot, out var item))
            return false;

        var component = item.BaseType.GetComponent<EquipComponent>();

        component?.Unequip();

        return equipped.Remove(slot);
    }
}