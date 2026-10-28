using System;
using System.Linq;
using UnityEngine;

public class PlayerInventory : BaseInventory, ITargetable
{
    public override int Rows => 5;
    public override int Columns => 8;

    private Loadout _loadout;
    public Loadout Loadout => _loadout ??= new Loadout(GetComponent<IEntity>());


    public event Action<IInventoryItem> OnItemDropped;
    public Action OnHeldItemChanged { get; internal set; }
    private IInventoryItem _heldItem;
    public IInventoryItem HeldItem
    {
        get => _heldItem;
        private set
        {
            _heldItem = value;
            OnHeldItemChanged?.Invoke();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Loadout.OnItemUnequipped += item => TryAdd(item);
        Loadout.CanUnequipToDestination = CanAdd;
    }

    public void SlotRightClicked(int row, int column)
    {
        if (!TryGet(row, column, out IInventoryItem inventoryItem))
            return;

        if (inventoryItem is not ItemDropInstance item)
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



    public void HandleLeftClick(int row, int column, bool ctrl)
    {
        if (ctrl)
        {
            if (TryGet(row, column, out IInventoryItem slotItem) && TryRemove(slotItem))
                OnItemDropped?.Invoke(slotItem);
            return;
        }

        if (HeldItem != null)
        {
            if (TryGet(row, column, out IInventoryItem slotItem))
            {
                if (TryRemove(slotItem))
                {
                    if (TryAddAt(HeldItem, row, column))
                    {
                        HeldItem = slotItem;
                    }
                    else
                        TryAddAt(slotItem, row, column);
                }
            }
            else if (TryAddAt(HeldItem, row, column))
            {
                HeldItem = null;
            }
            return;
        }

        if (TryGet(row, column, out IInventoryItem itemToPick) && TryRemove(itemToPick))
            HeldItem = itemToPick;
    }





    public TagContainer GetTargetingRequirements()
    {
        throw new NotImplementedException();
    }

    public InventoryResponse ApplyTargetedEffect(ItemDropInstance targetItem)
    {
        throw new NotImplementedException();
    }

    public void StartTargeting(ItemDropInstance orbInstance, TagContainer requirements, Action<ItemDropInstance> onTargetSelected)
    {
        throw new NotImplementedException();
    }


    public void DropHeld()
    {
        if (HeldItem == null) return;
        var item = HeldItem;
        HeldItem = null;
        OnItemDropped?.Invoke(item);
    }

}
