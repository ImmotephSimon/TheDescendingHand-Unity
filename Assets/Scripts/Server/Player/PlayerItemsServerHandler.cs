
using FishNet.Object;
using System.Collections.Generic;

public class PlayerItemsServerHandler : NetworkBehaviour, IPlayerItemsServerHandler
{
    private PlayerInventory _inventory;
    private Loadout _loadout;
    private PlayerItemsNetwork _network;
    private ITooltipPresenter tooltipPresenter;

    public int InventoryRows => _inventory.Rows;
    public int InventoryColumns => _inventory.Columns;

    public void Initialize(PlayerItemsNetwork network)
    {
        _network = network;

        _inventory = GetComponent<PlayerInventory>();
        _loadout = _inventory.Loadout;

        _inventory.OnChanged += SyncInventory;
        _inventory.OnHeldItemChanged += SyncHeldItem;
        _loadout.OnLoadoutChanged += SyncLoadout;

        _network.InventoryTooltipReceived += (row, col, dto) => TooltipPresenter.Current?.Show(dto);
        _network.EquippedTooltipReceived += (slot, dto) => TooltipPresenter.Current?.Show(dto);

        SyncInventory();
        SyncHeldItem();
        SyncLoadout();
    }


    public override void OnStopServer()
    {
        if (_inventory != null)
        {
            _inventory.OnChanged -= SyncInventory;
            _inventory.OnHeldItemChanged -= SyncHeldItem;
        }

        if (_loadout != null)
            _loadout.OnLoadoutChanged -= SyncLoadout;

        base.OnStopServer();
    }

    private void SyncInventory()
    {
        if (!IsServerInitialized)
            return;

        var items = new List<InventoryItemDto>();

        foreach (var entry in _inventory.GetItemPositions())
        {
            IInventoryItem item = entry.Key;

            items.Add(new InventoryItemDto
            {
                Icon = item.Icon,
                Position = entry.Value,
                Size = item.Size
            });
        }
        _network.ServerSetInventoryItems(items);
    }

    private void SyncLoadout()
    {
        if (!IsServerInitialized)
            return;

        var list = new List<EquippedItemDto>();

        foreach (var entry in _loadout.Equipped)
        {
            list.Add(new EquippedItemDto
            {
                Slot = entry.Key,
                BaseTypeId = entry.Value.BaseType.Id,
                Icon = entry.Value.BaseType.Appearance.Icon
            });
        }

        _network.ServerSetEquippedItems(list);
    }

    private void SyncHeldItem()
    {
        if (!IsServerInitialized)
            return;

        var held = _inventory.HeldItem;

        _network.ServerSetHeldItem(held == null
            ? default
            : new HeldItemDto
            {
                HasItem = true,
                Icon = held.Icon,
                Size = held.Size
            });
    }

    public void HandleInventorySlotClick(int row, int column, bool ctrl)
    {
        if (!ValidSlot(row, column))
            return;

        _inventory.HandleLeftClick(row, column, ctrl);
    }

    public void HandleInventorySlotRightClick(int row, int column)
    {
        if (!ValidSlot(row, column))
            return;

        _inventory.SlotRightClicked(row, column);
    }

    public void HandleUnequip(EquipmentSlot slot)
    {
        _loadout.Unequip(slot);
    }

    public void HandleInventoryTooltip(int row, int column)
    {
        if (!ValidSlot(row, column))
            return;

        if (_inventory.TryGet(row, column, out IInventoryItem item))
        {
            _network.ServerSendInventoryTooltip(row, column, item.Display());
        }
    }

    public void HandleEquippedTooltip(EquipmentSlot slot)
    {
        if (_loadout.Equipped.TryGetValue(slot, out var item))
            _network.ServerSendEquippedTooltip(slot, item.Display());
    }

    public void HandleDropHeld()
    {
        _inventory.DropHeld();
    }

    private bool ValidSlot(int row, int column)
    {
        return _inventory != null
            && row >= 0 && row < _inventory.Rows
            && column >= 0 && column < _inventory.Columns;
    }


}
