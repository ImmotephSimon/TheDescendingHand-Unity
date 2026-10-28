using System;
using UnityEngine;

[Serializable]
public class EquipComponentDefinition : ItemComponentDefinition
{
    [SerializeField] private EquipmentSlot slot;
    public EquipmentSlot Slot => slot;

    public override ItemUseComponent CreateRuntimeComponent()
    {
        return new EquipUseComponent(this);
    }
}