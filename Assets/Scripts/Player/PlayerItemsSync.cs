using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerItemsSync : NetworkBehaviour
{




    private PlayerInventory _inventory;
    private Loadout _loadout;
    private readonly SyncList<InventoryItemDto> _inventoryItems = new();

    private readonly SyncList<ItemTooltipDto> _equippedItems = new();

    public event Action InventoryChanged;
    public event Action LoadoutChanged;

    private readonly SyncVar<int> _inventoryRows = new(4);
    private readonly SyncVar<int> _inventoryColumns = new(4);

    public int InventoryRows => _inventoryRows.Value;
    public int InventoryColumns => _inventoryColumns.Value;


    public IReadOnlyList<InventoryItemDto> InventoryItems => _inventoryItems;
    public IReadOnlyList<ItemTooltipDto> EquippedItems => _equippedItems;

    public override void OnStartServer()
    {
        base.OnStartServer();

        _inventory = GetComponent<PlayerInventory>();
        _inventoryRows.Value = _inventory.Rows;
        _inventoryColumns.Value = _inventory.Columns;
        _loadout = _inventory.Loadout;

        _inventory.OnChanged += SyncInventory;
        _loadout.OnLoadoutChanged += HandleLoadoutChanged;

        SyncInventory();
        HandleLoadoutChanged();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _inventoryItems.OnChange += HandleInventoryChanged;
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




    /// <summary>
    /// recently refactored, so keep
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="item"></param>
    /// <returns></returns>
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
    public void RequestInventorySlotClick(int row, int column)
    {
        _inventory.SlotLeftClicked(row, column);
    }

    [ServerRpc]
    public void RequestInventorySlotRightClick(int row, int column)
    {
        _inventory.SlotRightClicked(row, column);
    }


    //private void HandleLoadoutChanged()
    //{
    //    if (IsServerStarted)
    //    {
    //        _equippedItems.Clear();

    //        foreach (var entry in _loadout.Equipped)
    //        {
    //            var item = entry.Value;

    //            _equippedItems.Add(new ItemTooltipDto
    //            {
    //                EquipmentTypeId = entry.Key.ID,
    //                BaseTypeId = item.BaseType.ID,
    //                RarityId = item.Rarity.Id,
    //                Explicits = item.Explicits
    //                    .Select(affix => new AffixState
    //                    {
    //                        DefinitionId = affix.Definition.Id,
    //                        Tier = affix.Tier
    //                    })
    //                    .ToList()
    //            });
    //        }
    //    }

    //    LoadoutChanged?.Invoke();
    //}
    private void HandleLoadoutChanged()
    {
        if (IsServerStarted)
        {
            _equippedItems.Clear();

            foreach (var entry in _loadout.Equipped)
            {
                _equippedItems.Add(CreateItemTooltip(entry.Value));
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
        if (!_inventory.TryGet(row, column, out IInventoryItem item))
            return;

        if (item is not ItemDropInstance itemDrop)
        {
            Debug.LogError($"visualizing cards unhandled");
            return;
        }

        var dto = CreateItemTooltip(itemDrop);

        Target_InventoryTooltipRpc(Owner, dto);
    }

    [TargetRpc]
    private void Target_InventoryTooltipRpc(NetworkConnection connection, ItemTooltipDto dto)
    {
        TooltipController.Instance.ShowItem(dto);
    }

    private ItemTooltipDto CreateItemTooltip(ItemDropInstance item)
    {
        var equipUse = item.BaseType.Components
            .OfType<EquipComponentDefinition>()
            .FirstOrDefault();

        Debug.Assert(equipUse != null, $"CreateItemTooltip for item which can't be equipped.");

        return new ItemTooltipDto
        {
            EquipmentTypeId = equipUse.EquipmentType.ID,
            BaseTypeId = item.BaseType.Id,
            RarityId = item.Rarity.Id,

            Implicits = item.BaseType.Implicits
                .Select(AffixState.FromModifier)
                .ToList(),

            Explicits = item.Explicits
                .Select(AffixState.FromInstance)
                .ToList()
        };
    }
}