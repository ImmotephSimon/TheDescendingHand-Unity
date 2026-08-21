using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInventory : BaseInventory, ITargetable
{
    public override int Rows => 6;
    public override int Columns => 8;

    private Loadout _loadout;
    public Loadout Loadout => _loadout ??= new Loadout(GetComponent<IEntity>());

    public PlayerInventory()
    {   
        
    }
    protected override void Awake()
    {
        base.Awake();
        Loadout.OnItemUnequipped += item => TryAdd(item);
        Loadout.CanUnequipToDestination = CanAdd;
    }
    
    public override void SlotRightClicked(int row, int column)
    {
        if (!TryGet(row, column, out IInventoryItem inventoryItem))
            return;

        if (inventoryItem is not ItemInstance item)
        {
            Debug.LogWarning($"Can't equip {inventoryItem}");
            return;
        }
        if (!item.BaseType.Components.Any(x => x is EquipComponentDefinition))
        {
            Debug.Log($"Item cannot be equipped.");
            return;
        }

        if (!Loadout.Equip(item))
        {
            Debug.LogWarning($"Loadout returned false.");
            return;
        }
        if (!TryRemove(item))
        {
            Debug.LogError($"Failed to remove item.");
            return;
        }
    }
    public override void SlotLeftClicked(int row, int column)
    {
        var cursorController = CursorItemController.Instance;
        if (cursorController == null || cursorController.CursorItem == null) return;

        CursorItem cursor = cursorController.CursorItem;

        // Drop shortcut (Ctrl+Click)
        bool isCtrlPressed = Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed;
        if (isCtrlPressed)
        {
            if (TryGet(row, column, out IInventoryItem slotItem))
            {
                DropInWorld(slotItem);
            }
            return;
        }

        // Cursor holds an item: place or swap
        if (cursor.HasItem)
        {
            IInventoryItem held = cursor.HeldItem;
            

            if (TryGet(row, column, out IInventoryItem slotItem))
            {
                // Swap slot item with cursor item
                if (TryRemove(slotItem))
                {
                    if (TryAddAt(held, row, column))
                    {
                        cursor.Clear();
                        cursor.Hold(slotItem);
                    }
                    else
                    {
                        // Re-add original if target placement fails
                        TryAddAt(slotItem, row, column);
                    }
                }
            }
            else if (TryAddAt(held, row, column))
            {
                cursor.Clear();
            }
            return;
        }

        // Cursor is empty: pick up slot item
        if (TryGet(row, column, out IInventoryItem itemToPick))
        {
            CursorItemController.Instance.OnDropRequested -= DropInWorld;
            CursorItemController.Instance.OnDropRequested += DropInWorld;
            cursor.Hold(itemToPick);
        }
    }

    private void DropInWorld(IInventoryItem instance)
    {
        ClientBridge.Instance.PlayerNetwork.RequestDrop(instance.Id.ToString());
        CursorItemController.Instance.CursorItem.Clear();
    }

    public TagContainer GetTargetingRequirements()
    {
        throw new NotImplementedException();
    }

    public InventoryResponse ApplyTargetedEffect(ItemInstance targetItem)
    {
        throw new NotImplementedException();
    }

    public void StartTargeting(ItemInstance orbInstance, TagContainer requirements, Action<ItemInstance> onTargetSelected)
    {
        throw new NotImplementedException();
    }

    private void OnDestroy()
    {
        if (CursorItemController.Instance != null)
            CursorItemController.Instance.OnDropRequested -= DropInWorld;
    }
}
