using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemsSync : NetworkBehaviour
{
    public struct EquippedItemDto
    {
        public Guid EquipmentTypeId;
        public Guid BaseTypeId;
    }



    private PlayerInventory _inventory;
    private Loadout _loadout;
    private readonly SyncList<InventoryItemDto> _inventoryItems = new();

    private readonly SyncList<EquippedItemDto> _equippedItems = new();

    public event Action InventoryChanged;
    public event Action LoadoutChanged;

    private readonly SyncVar<int> _inventoryRows = new(4);
    private readonly SyncVar<int> _inventoryColumns = new(4);

    public int InventoryRows => _inventoryRows.Value;
    public int InventoryColumns => _inventoryColumns.Value;


    public IReadOnlyList<InventoryItemDto> InventoryItems => _inventoryItems;
    public IReadOnlyList<EquippedItemDto> EquippedItems => _equippedItems;

    public struct HeldItemDto
    {
        public bool HasItem;
        public Guid ItemId;
        public Vector2Int Size;
    }

    private readonly SyncVar<HeldItemDto> _heldItem = new();
    public HeldItemDto HeldItem => _heldItem.Value;
    public event Action HeldItemChanged;


    public override void OnStartServer()
    {
        base.OnStartServer();

        _inventory = GetComponent<PlayerInventory>();
        _inventoryRows.Value = _inventory.Rows;
        _inventoryColumns.Value = _inventory.Columns;
        _loadout = _inventory.Loadout;

        _inventory.OnChanged += SyncInventory;
        _loadout.OnLoadoutChanged += HandleLoadoutChanged;
        _inventory.OnHeldItemChanged += SyncHeldItem;

        SyncInventory();
        HandleLoadoutChanged();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _inventoryItems.OnChange += HandleInventoryChanged;

        _heldItem.OnChange += (prev, next, asServer) =>
        {
            if (!asServer) HeldItemChanged?.Invoke();
        };

    }

    private void OnDestroy()
    {
        if (_inventory != null)
            _inventory.OnChanged -= SyncInventory;

        if (_loadout != null)
            _loadout.OnLoadoutChanged -= HandleLoadoutChanged;

        _inventoryItems.OnChange -= HandleInventoryChanged;
    }

    [Server]
    private void SyncInventory()
    {
        _inventoryItems.Clear();

        foreach (var entry in _inventory.GetItemPositions())
        {
            IInventoryItem item = entry.Key;
            Vector2Int position = entry.Value;

            InventoryItemDto dto = new InventoryItemDto
            {
                ItemId = item.InventoryId,
                Position = position,
                Size = item.Size
            };

            _inventoryItems.Add(dto);
        }
    }

    private void HandleInventoryChanged(
        SyncListOperation op,
        int index,
        InventoryItemDto oldItem,
        InventoryItemDto newItem,
        bool asServer)
    {
        if (!asServer)
            InventoryChanged?.Invoke();
    }



    public bool TryGetInventoryItem(
        int row,
        int column,
        out InventoryItemDto item)
    {
        foreach (var dto in _inventoryItems)
        {
            var origin = dto.Position;
            var size = dto.Size;

            if (column < origin.x ||
                column >= origin.x + size.x ||
                row < origin.y ||
                row >= origin.y + size.y)
                continue;

            item = dto;
            return true;
        }

        item = default;
        return false;
    }




    [ServerRpc]
    public void RequestInventorySlotClick(int row, int column, bool ctrl)
    {
        if (row < 0 || row >= _inventory.Rows || column < 0 || column >= _inventory.Columns)
            return;

        _inventory.HandleLeftClick(row, column, ctrl);
    }

    [ServerRpc]
    public void RequestInventorySlotRightClick(int row, int column)
    {
        _inventory.SlotRightClicked(row, column);
    }


    private void HandleLoadoutChanged()
    {
        if (IsServerStarted)
        {
            _equippedItems.Clear();

            foreach (var entry in _loadout.Equipped)
            {
                _equippedItems.Add(new EquippedItemDto
                {
                    EquipmentTypeId = entry.Key.ID,
                    BaseTypeId = entry.Value.BaseType.Id
                });
            }
        }

        LoadoutChanged?.Invoke();
    }

    [ServerRpc]
    public void Server_RequestUnequip(Guid equipmentTypeId)
    {
        Debug.Log($"Unequip request: {equipmentTypeId}");
        foreach (var entry in _loadout.Equipped)
            Debug.Log($"Equipped: {entry.Key} -> {entry.Value.BaseType.name}");

        EquipmentType.TryGet(equipmentTypeId, out EquipmentType equipmentSlot);
        _loadout.Unequip(equipmentSlot);
    }


    [ServerRpc]
    public void Server_RequestInventoryTooltip(int row, int column)
    {
        if (_inventory.TryGet(row, column, out IInventoryItem item))
            Target_TooltipRpc(Owner, item.Display());
    }

    [ServerRpc]
    public void Server_RequestEquippedTooltip(Guid equipmentTypeId)
    {
        if (!EquipmentType.TryGet(equipmentTypeId, out EquipmentType slot))
            return;

        if (_loadout.Equipped.TryGetValue(slot, out var item))
            Target_TooltipRpc(Owner, item.Display());
    }

    [TargetRpc]
    private void Target_TooltipRpc(NetworkConnection connection, TooltipDto dto)
    {
        TooltipController.Instance.Show(dto);
    }

    [Server]
    private void SyncHeldItem()
    {
        var held = _inventory.HeldItem;
        _heldItem.Value = held == null
            ? default
            : new HeldItemDto { HasItem = true, ItemId = held.InventoryId, Size = held.Size };
    }

    [ServerRpc]
    public void RequestDropHeld() => _inventory.DropHeld();


}