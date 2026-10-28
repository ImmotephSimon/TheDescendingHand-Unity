using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentTypeRegistry : MonoBehaviour
{
    [SerializeField] private EquipmentTypeDefinition[] equipmentTypes;

    private Dictionary<EquipmentSlot, EquipmentTypeDefinition> bySlot;

    private void Awake()
    {
        bySlot = new Dictionary<EquipmentSlot, EquipmentTypeDefinition>();

        foreach (var def in equipmentTypes)
        {
            if (def == null)
                throw new InvalidOperationException(
                    "EquipmentTypeRegistry contains a null definition.");

            if (def.ModifierPool == null)
                throw new InvalidOperationException(
                    $"{def.name} ({def.Slot}) has no ModifierPool.");

            if (!bySlot.TryAdd(def.Slot, def))
                throw new InvalidOperationException(
                    $"Duplicate EquipmentTypeDefinition for {def.Slot}.");
        }

        foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
        {
            if (!bySlot.ContainsKey(slot))
                throw new InvalidOperationException(
                    $"No EquipmentTypeDefinition for {slot}.");
        }
    }

    public bool TryGet(EquipmentSlot slot, out EquipmentTypeDefinition definition)
    {
        if (bySlot.TryGetValue(slot, out definition))
            return true;

        Debug.LogError($"No EquipmentTypeDefinition registered for {slot}.", this);
        return false;
    }
}