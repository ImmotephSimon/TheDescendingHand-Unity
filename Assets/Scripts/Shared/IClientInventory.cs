using System;
using System.Collections.Generic;
using UnityEngine;

public interface IClientInventory
{
    int Rows { get; }
    int Columns { get; }
    IClientLoadout Loadout { get; }

    event Action OnHeldItemChanged;
    event Action OnChanged;

    IReadOnlyDictionary<IClientInventoryItem, Vector2Int> GetItemPositions();
    void HandleLeftClick(int row, int column, bool ctrl);
    void SlotRightClicked(int row, int column);

    IClientInventoryItem HeldItem { get; }
}

public interface IClientInventoryItem
{
    Guid InventoryId { get; }
    Vector2Int Size { get; }
}