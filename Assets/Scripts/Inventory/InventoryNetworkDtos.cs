using System;
using System.Collections.Generic;
using UnityEngine;




[Serializable]
public struct InventoryItemDto
{
    public Guid ItemId;
    public Vector2Int Position;
    public Vector2Int Size;
}

[Serializable]
public struct CardTooltipDto
{
    public string CardId;
    // Add any dynamic card state (e.g., Level, Foil, XP)
}

[Serializable]
public struct ItemTooltipDto
{
    public Guid EquipmentTypeId;
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
    public float RolledValue;
    public GameTag Modifier;
    public MathOp MathOp;
    public TagRequirement TagRequirement;

    public static AffixState FromInstance(AffixInstance instance)
    {
        return new AffixState
        {
            DisplayName = instance.Definition.NameOverride.Length > 0
                ? instance.Definition.NameOverride
                : string.Empty,
            Tier = instance.Tier,
            RolledValue = instance.Value,
            Modifier = instance.Definition.Modifier,
            MathOp = instance.Definition.MathOp,
            TagRequirement = instance.Definition.TagRequirement
        };
    }

    public static AffixState FromModifier(StatModifier modifier)
    {
        return new AffixState
        {
            RolledValue = modifier.Value,
            Modifier = modifier.Stat,
            MathOp = modifier.Op,
            TagRequirement = modifier.RequiredTags
        };
    }
}