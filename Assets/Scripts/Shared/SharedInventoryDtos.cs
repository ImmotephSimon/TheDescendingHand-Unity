using System;
using UnityEngine;

[Serializable]
public struct InventoryItemDto
{
    public SpriteString Icon;
    public Vector2Int Position;
    public Vector2Int Size;
}

public struct EquippedItemDto
{
    public EquipmentSlot Slot;
    public Guid BaseTypeId;
    public SpriteString Icon;
}


public struct HeldItemDto
{
    public bool HasItem;
    public SpriteString Icon;
    public Vector2Int Size;
}