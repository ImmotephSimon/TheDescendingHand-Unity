using System;
using System.Collections.Generic;


[Serializable]
public struct ItemTooltipDto
{
    public EquipmentSlot EquipmentSlot;
    public Guid BaseTypeId;
    public Guid RarityId;
    public List<AffixState> Implicits;
    public List<AffixState> Explicits;
}

[Serializable]
public struct AffixState
{
    public string DisplayName;
    public float Tier;
    public List<StatModifier> ResolvedMods;
    public TagRequirement TagRequirement;

    public static AffixState FromInstance(AffixInstance instance, List<ItemImplicit> itemImplicits)
    {
        return new AffixState
        {
            DisplayName = instance.Definition.NameOverride ?? string.Empty,
            Tier = instance.Tier,
            ResolvedMods = instance.ToStatModifiers(itemImplicits),
            TagRequirement = instance.Definition.ContextRequirement
        };
    }

    public static AffixState FromModifier(StatModifier modifier)
    {
        return new AffixState
        {
            DisplayName = string.Empty,
            Tier = 1f,
            ResolvedMods = new List<StatModifier> { modifier },
            TagRequirement = modifier.RequiredTags
        };
    }
}