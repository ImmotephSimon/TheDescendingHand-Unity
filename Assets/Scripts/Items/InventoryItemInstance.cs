using System;
using UnityEngine;

public abstract class InventoryItemInstance : IInventoryItem
{
    public abstract Guid InventoryId { get; protected set; }
    public abstract Vector2Int Size { get; }
    public abstract Sprite Icon { get; }

    public abstract void Display();
}