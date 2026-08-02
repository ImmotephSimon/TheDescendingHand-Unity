using System;
using System.Collections.Generic;
using UnityEngine;

public interface IInventory
{
    int Rows { get; }
    int Columns { get; }

    IReadOnlyDictionary<ItemInstance, Vector2Int> GetPlacedItems();
    ItemInstance Get(int row, int column);

    bool TryAdd(ItemInstance item);
    bool TryRemove(ItemInstance item);

    event Action OnChanged;
}