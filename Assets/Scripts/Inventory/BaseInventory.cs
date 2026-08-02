using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseInventory : MonoBehaviour, IInventory
{
    [SerializeField] protected int rows = 6;
    [SerializeField] protected int columns = 8;

    private ItemInstance[,] occupancyGrid;

    // Fast lookup for item top-left origin points
    private readonly Dictionary<ItemInstance, Vector2Int> itemPositions = new();

    public int Rows => rows;
    public int Columns => columns;
    public event Action OnChanged;

    private void Awake()
    {
        occupancyGrid = new ItemInstance[rows, columns];
    }

    public IReadOnlyDictionary<ItemInstance, Vector2Int> GetPlacedItems()
    {
        return itemPositions;
    }

    public ItemInstance Get(int row, int column)
    {
        if (!IsWithinGrid(row, column)) return null;
        return occupancyGrid[row, column];
    }
    public bool TryAdd(ItemInstance item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item), "Cannot add null ItemInstance to inventory.");

        if (item.BaseType == null)
            throw new InvalidOperationException($"[{nameof(TryAdd)}] ItemInstance missing BaseType definition.");

        if (itemPositions.ContainsKey(item))
        {
            Debug.LogWarning($"[{nameof(TryAdd)}] Item '{item.BaseType.name}' is already present in inventory.");
            return false;
        }

        if (!TryFindFreeSlot(item.BaseType.InventorySize, out Vector2Int origin))
            return false; // Grid full / no space

        OccupyCells(item, origin);
        OnChanged?.Invoke();
        return true;
    }

    public bool TryRemove(ItemInstance item)
    {
        if (item?.BaseType == null || !itemPositions.TryGetValue(item, out Vector2Int origin))
            return false;

        ClearCells(item, origin);
        itemPositions.Remove(item);
        OnChanged?.Invoke();
        return true;
    }

    public bool TryGetPosition(ItemInstance item, out Vector2Int origin)
    {
        return itemPositions.TryGetValue(item, out origin);
    }

    public bool CanPlaceAt(ItemInstance item, Vector2Int origin)
    {
        Vector2Int size = item.BaseType.InventorySize;
        if (origin.x < 0 || origin.y < 0 || origin.x + size.x > columns || origin.y + size.y > rows)
            return false;

        for (int r = 0; r < size.y; r++)
        {
            for (int c = 0; c < size.x; c++)
            {
                ItemInstance existing = occupancyGrid[origin.y + r, origin.x + c];
                if (existing != null && existing != item)
                    return false;
            }
        }
        return true;
    }

    private bool TryFindFreeSlot(Vector2Int size, out Vector2Int origin)
    {
        for (int r = 0; r <= rows - size.y; r++)
        {
            for (int c = 0; c <= columns - size.x; c++)
            {
                if (CanFitAt(r, c, size))
                {
                    origin = new Vector2Int(c, r);
                    return true;
                }
            }
        }

        origin = Vector2Int.zero;
        return false;
    }

    private bool CanFitAt(int startRow, int startCol, Vector2Int size)
    {
        for (int r = 0; r < size.y; r++)
        {
            for (int c = 0; c < size.x; c++)
            {
                if (occupancyGrid[startRow + r, startCol + c] != null)
                    return false;
            }
        }
        return true;
    }

    private void OccupyCells(ItemInstance item, Vector2Int origin)
    {
        Vector2Int size = item.BaseType.InventorySize;
        for (int r = 0; r < size.y; r++)
        {
            for (int c = 0; c < size.x; c++)
            {
                occupancyGrid[origin.y + r, origin.x + c] = item;
            }
        }
        itemPositions[item] = origin;
    }

    private void ClearCells(ItemInstance item, Vector2Int origin)
    {
        Vector2Int size = item.BaseType.InventorySize;
        for (int r = 0; r < size.y; r++)
        {
            for (int c = 0; c < size.x; c++)
            {
                occupancyGrid[origin.y + r, origin.x + c] = null;
            }
        }
    }

    private bool IsWithinGrid(int r, int c) => r >= 0 && r < rows && c >= 0 && c < columns;


}