using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipUseComponent : ItemUseComponent, IUsable
{
    public EquipComponentDefinition Definition { get; }

    public EquipUseComponent(EquipComponentDefinition definition)
    {
        Definition = definition;
    }

    public void Equip(ItemInstance instance, IEntity user)
    {
        instance.ApplyAffixes(user);
        ClientBridge.Instance.EquipmentVisuals.SetEquipment(Definition.EquipmentType, instance.BaseType);
    }

    public void Unequip(ItemInstance instance, IEntity user)
    {
        instance.ClearAffixes(user);
        ClientBridge.Instance.EquipmentVisuals.ClearEquipment(Definition.EquipmentType);
    }

    public override void Use(ItemInstance instance, IEntity user)
    {
        throw new InvalidOperationException($"[Item] {instance.BaseType.name} is an equipment item and cannot be used directly.");
    }
}