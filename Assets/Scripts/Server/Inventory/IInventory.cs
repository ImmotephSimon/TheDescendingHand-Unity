using System;
using System.Collections.Generic;
using UnityEngine;

public interface IInventory
{
    int Rows { get; }
    int Columns { get; }
    bool TryGet(Guid id, out IInventoryItem item);
    IReadOnlyDictionary<IInventoryItem, Vector2Int> GetItemPositions();
    bool TryGet(int row, int column, out IInventoryItem item);

    bool TryAdd(IInventoryItem item);
    bool TryRemove(IInventoryItem item);

    event Action OnChanged;
}