using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private InventorySlotView slotPrefab;
    [SerializeField] private ItemIconView itemIconPrefab;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private Vector2 cellSize = new Vector2(64f, 64f);
    private Vector2 spacing = new Vector2(2f, 2f);

    private IInventory inventory;
    private InventorySlotView[,] slots;
    private readonly List<ItemIconView> spawnedIcons = new List<ItemIconView>();

    public void Bind(Player player)
    {
        if (inventory != null)
            inventory.OnChanged -= Refresh;

        inventory = player.GetComponent<IInventory>();
        if (inventory == null)
            return;

        inventory.OnChanged += Refresh;

        InitializeGrid();
        Refresh();
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnChanged -= Refresh;
    }

    private void InitializeGrid()
    {
        // Clear existing slot children if re-initializing
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        int rows = inventory.Rows;
        int cols = inventory.Columns;

        if (TryGetComponent<GridLayoutGroup>(out var gridLayout))
        {
            gridLayout.cellSize = cellSize;
            gridLayout.spacing = spacing;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = cols;
        }

        slots = new InventorySlotView[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                slots[r, c] = Instantiate(slotPrefab, transform);
                slots[r, c].Initialize(r, c); 
            }
        }
    }

    private void Refresh()
    {
        // Clear active item UI instances
        for (int i = spawnedIcons.Count - 1; i >= 0; i--)
        {
            Destroy(spawnedIcons[i].gameObject);
        }
        spawnedIcons.Clear();

        Transform parent = itemContainer != null ? itemContainer : transform;

        // Render multi-tile items
        foreach (var entry in inventory.GetPlacedItems())
        {
            ItemInstance item = entry.Key;
            Vector2Int origin = entry.Value;

            ItemIconView iconInstance = Instantiate(itemIconPrefab, parent);
            spawnedIcons.Add(iconInstance);

            Vector2 position = GetLocalPosition(origin.x, origin.y);

            Vector2Int inventorySize = item.BaseType.InventorySize;

            Vector2 size = new Vector2(
                inventorySize.x * cellSize.x + (inventorySize.x - 1) * spacing.x,
                inventorySize.y * cellSize.y + (inventorySize.y - 1) * spacing.y
            );

            iconInstance.Render(
                item.BaseType.Appearance.Icon,
                position,
                size
            );
        }
    }

    private Vector2 GetLocalPosition(int column, int row)
    {
        float x = column * (cellSize.x + spacing.x);
        float y = -row * (cellSize.y + spacing.y);
        return new Vector2(x, y);
    }

    

    internal void ToggleVisibility()
    {
        panelRoot.SetActive(!panelRoot.activeSelf);
    }
}