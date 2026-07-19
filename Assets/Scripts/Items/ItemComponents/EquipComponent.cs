using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Items/Components/Equip")]
public class EquipComponent : ItemComponent
{
    [SerializeField] private List<StatModifier> modifiers;

    private readonly List<ModifierHandle> handles = new();

    private StatModifierComponent ownerStats;
    public void Equip(StatModifierComponent stats)
    {
        ownerStats = stats;

        handles.Clear();

        foreach (var modifier in modifiers)
        {
            ModifierHandle handle = ownerStats.AddModifier(modifier);
            handles.Add(handle);
        }
    }


    public void Unequip()
    {
        if (ownerStats == null)
            return;

        foreach (var handle in handles)
        {
            ownerStats.RemoveModifier(handle);
        }

        handles.Clear();
        ownerStats = null;
    }

    public override void Activate(Character user)
    {
        if (user is IEquipmentWearer wearer)
            wearer.Equip(item);
    }
}