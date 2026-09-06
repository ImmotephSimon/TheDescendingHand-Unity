using System;
using UnityEngine;

public interface IInventoryItem
{
    Guid InventoryId { get; }
    Vector2Int Size { get; }
    Sprite Icon { get; }

    public void Display();
}

