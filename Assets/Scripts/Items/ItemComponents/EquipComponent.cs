using System.Collections.Generic;
using UnityEngine;

public enum SlotInfo { Default, TwoHanded}

public class EquipComponent : ItemComponent
{
    [SerializeField] private EquipmentType equipmentType;
    [SerializeField] private SlotInfo slotInfo = SlotInfo.Default;

    private List<StatModifier> modifiers;
    private IEntity _owner;
    private readonly List<ModifierHandle> handles = new();

    public EquipmentType EquipmentType => equipmentType;

    public void Equip()
    {
        foreach (var modifier in modifiers)
        {
            ModifierHandle handle = _owner.Stats.AddModifier(modifier);
            handles.Add(handle);
        }
        item.SetVisible(true);
    }


    public void Unequip()
    {

        foreach (var handle in handles)
        {
            _owner.Stats.RemoveModifier(handle);
        }

        handles.Clear();
        item.SetVisible(false);
    }

    public override void Activate(IEntity owner)
    {
        _owner = owner;
    }
}