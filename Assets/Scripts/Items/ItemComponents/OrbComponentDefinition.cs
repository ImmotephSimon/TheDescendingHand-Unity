using System;
using UnityEngine;

public enum OrbEffect { Corruption, AddPhys, AddFire, AddCold, AddLightning }

[Serializable]
public class OrbComponentDefinition : ItemComponentDefinition
{
    [SerializeField] private TagContainer targetRequirements; // Supports complex combinations (e.g., Magic + Weapon)
    [SerializeField] private OrbEffect orbEffect;
    public TagContainer TargetRequirements => targetRequirements;

    public override ItemUseComponent CreateRuntimeComponent()
    {
        return new OrbUseComponent(this);
    }

    public InventoryResponse ApplyEffect(ItemInstance targetItem)
    {
        // TODO: Modify targetItem (e.g., add affix, reroll, etc.)

        //return orbEffect switch
        //{
        //    OrbEffect.Corruption => affixes.ApplyCorruption() ? InventoryResponse.Consumed : InventoryResponse.Failed,
        //    OrbEffect.AddPhys => affixes.AddAffixTag(DamageType.Physical) ? InventoryResponse.Consumed : InventoryResponse.Failed,
        //    _ => InventoryResponse.Failed
        //};
        return InventoryResponse.Consumed;
    }
}