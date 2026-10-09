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

    //[ServerRpc]
    //public void Server_RequestInventoryTooltip(int row, int column)
    //{
    //    if (!_inventory.TryGet(row, column, out IInventoryItem item))
    //        return;

    //    if (item is not ItemDropInstance itemDrop)
    //    {
    //        Debug.LogError($"visualizing cards unhandled");
    //        return;
    //    }

    //    var dto = item.Display();

    //    //switch (item)
    //    //{
    //    //    case ItemDropInstance itemDrop:
    //    //        Target_InventoryTooltipRpc(Owner, CreateItemTooltip(itemDrop));
    //    //        break;

    //    //    case CardDropInstance card:
    //    //        Target_CardTooltipRpc(Owner, card.Definition.Id);
    //    //        break;

    //    //    default:
    //    //        Debug.LogError($"Unhandled tooltip type: {item.GetType().Name}");
    //    //        break;
    //    //}


    //    //var dto = CreateItemTooltip(itemDrop);

    //    Target_InventoryTooltipRpc(Owner, dto);
    //}

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

    //private ItemTooltipDto CreateItemTooltip(ItemDropInstance item)
    //{
    //    var equipUse = item.BaseType.Components
    //        .OfType<EquipComponentDefinition>()
    //        .FirstOrDefault();

    //    Debug.Assert(equipUse != null, $"CreateItemTooltip for item which can't be equipped.");

    //    return new ItemTooltipDto
    //    {
    //        EquipmentTypeId = equipUse.EquipmentType.ID,
    //        BaseTypeId = item.BaseType.Id,
    //        RarityId = item.Rarity.Id,

    //        Implicits = item.BaseType.Implicits.Select(x => x.Modifier)
    //            .Select(AffixState.FromModifier)
    //            .ToList(),

    //        Explicits = item.Explicits
    //            .Select(affix => AffixState.FromInstance(affix, item.BaseType.Implicits))
    //            .ToList()
    //    };
    //}
}