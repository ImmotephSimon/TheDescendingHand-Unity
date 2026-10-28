using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;

public class PlayerItemsNetwork : NetworkBehaviour
{
    private readonly SyncList<InventoryItemDto> _inventoryItems = new();
    private readonly SyncList<EquippedItemDto> _equippedItems = new();
    private readonly SyncVar<HeldItemDto> _heldItem = new();

    private readonly SyncVar<int> _inventoryRows = new(4);
    private readonly SyncVar<int> _inventoryColumns = new(4);

    // Client-side lookup, rebuilt whenever the loadout batch completes.
    private readonly Dictionary<EquipmentSlot, EquippedItemDto> _equippedBySlot = new();

    private IPlayerItemsServerHandler _serverHandler;

    public int InventoryRows => _inventoryRows.Value;
    public int InventoryColumns => _inventoryColumns.Value;

    public IReadOnlyList<InventoryItemDto> InventoryItems => _inventoryItems;
    public IReadOnlyList<EquippedItemDto> EquippedItems => _equippedItems;
    public HeldItemDto HeldItem => _heldItem.Value;

    public event Action InventoryChanged;
    public event Action LoadoutChanged;
    public event Action HeldItemChanged;

    // Tagged by source so each view only accepts its own response.
    public event Action<EquipmentSlot, TooltipDto> EquippedTooltipReceived;
    public event Action<int, int, TooltipDto> InventoryTooltipReceived; // row, column, dto

    // ---------- Lifecycle ----------

    public override void OnStartServer()
    {
        base.OnStartServer();

        _serverHandler = GetComponent<IPlayerItemsServerHandler>();
        if (_serverHandler == null)
        {
            throw new InvalidOperationException(
                $"{nameof(IPlayerItemsServerHandler)} is missing on {gameObject.name}.");
        }

        _serverHandler.Initialize(this);

        _inventoryRows.Value = _serverHandler.InventoryRows;
        _inventoryColumns.Value = _serverHandler.InventoryColumns;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _inventoryItems.OnChange += OnInventoryChanged;
        _equippedItems.OnChange += OnLoadoutChanged;
        _heldItem.OnChange += OnHeldItemChanged;
    }

    public override void OnStopClient()
    {
        _inventoryItems.OnChange -= OnInventoryChanged;
        _equippedItems.OnChange -= OnLoadoutChanged;
        _heldItem.OnChange -= OnHeldItemChanged;

        _equippedBySlot.Clear();

        base.OnStopClient();
    }

    // ---------- Server-only state updates (called by PlayerItemsSync) ----------

    [Server]
    public void ServerSetInventoryItems(IEnumerable<InventoryItemDto> items)
    {
        _inventoryItems.Clear();
        foreach (var item in items)
            _inventoryItems.Add(item);
    }

    [Server]
    public void ServerSetEquippedItems(IEnumerable<EquippedItemDto> items)
    {
        _equippedItems.Clear();
        foreach (var item in items)
            _equippedItems.Add(item);
    }

    [Server]
    public void ServerSetHeldItem(HeldItemDto item) => _heldItem.Value = item;

    [Server]
    public void ServerSendEquippedTooltip(EquipmentSlot slot, TooltipDto dto)
    {
        if (Owner != null)
            TargetEquippedTooltipRpc(Owner, slot, dto);
    }

    [Server]
    public void ServerSendInventoryTooltip(int row, int column, TooltipDto dto)
    {
        if (Owner != null)
            TargetInventoryTooltipRpc(Owner, row, column, dto);
    }

    [TargetRpc]
    private void TargetEquippedTooltipRpc(NetworkConnection connection, EquipmentSlot slot, TooltipDto dto)
        => EquippedTooltipReceived?.Invoke(slot, dto);

    [TargetRpc]
    private void TargetInventoryTooltipRpc(NetworkConnection connection, int row, int column, TooltipDto dto)
        => InventoryTooltipReceived?.Invoke(row, column, dto);

    // ---------- Client-side change handling ----------
    // Only react on Complete: the server does Clear() + N Add()s per update,
    // and we don't want N+1 refreshes through half-built states.

    private void OnInventoryChanged(
        SyncListOperation op, int index,
        InventoryItemDto oldItem, InventoryItemDto newItem, bool asServer)
    {
        if (asServer || op != SyncListOperation.Complete) return;
        InventoryChanged?.Invoke();
    }

    private void OnLoadoutChanged(
        SyncListOperation op, int index,
        EquippedItemDto oldItem, EquippedItemDto newItem, bool asServer)
    {
        if (asServer || op != SyncListOperation.Complete) return;

        _equippedBySlot.Clear();
        foreach (EquippedItemDto dto in _equippedItems)
            _equippedBySlot[dto.Slot] = dto;

        LoadoutChanged?.Invoke();
    }

    private void OnHeldItemChanged(HeldItemDto previous, HeldItemDto next, bool asServer)
    {
        if (!asServer)
            HeldItemChanged?.Invoke();
    }

    // ---------- Client queries ----------

    public bool TryGetEquipped(EquipmentSlot slot, out EquippedItemDto item)
        => _equippedBySlot.TryGetValue(slot, out item);

    public bool TryGetInventoryItem(int row, int column, out InventoryItemDto item)
    {
        foreach (var dto in _inventoryItems)
        {
            var origin = dto.Position;
            var size = dto.Size;

            if (column < origin.x || column >= origin.x + size.x ||
                row < origin.y || row >= origin.y + size.y)
                continue;

            item = dto;
            return true;
        }

        item = default;
        return false;
    }

    // ---------- Intents (client -> server). Never trust the arguments. ----------

    [ServerRpc]
    public void RequestInventorySlotClick(int row, int column, bool ctrl)
    {
        if (!IsValidCell(row, column)) return;
        _serverHandler.HandleInventorySlotClick(row, column, ctrl);
    }

    [ServerRpc]
    public void RequestInventorySlotRightClick(int row, int column)
    {
        if (!IsValidCell(row, column)) return;
        _serverHandler.HandleInventorySlotRightClick(row, column);
    }

    [ServerRpc]
    public void RequestInventoryTooltip(int row, int column)
    {
        if (!IsValidCell(row, column)) return;
        _serverHandler.HandleInventoryTooltip(row, column);
    }

    [ServerRpc]
    public void RequestUnequip(EquipmentSlot slot)
    {
        if (!IsValidSlot(slot)) return;
        _serverHandler.HandleUnequip(slot);
    }

    [ServerRpc]
    public void RequestEquippedTooltip(EquipmentSlot slot)
    {
        if (!IsValidSlot(slot)) return;
        _serverHandler.HandleEquippedTooltip(slot);
    }

    [ServerRpc]
    public void RequestDropHeld() => _serverHandler.HandleDropHeld();

    private bool IsValidCell(int row, int column)
        => row >= 0 && row < _inventoryRows.Value &&
           column >= 0 && column < _inventoryColumns.Value;

    private static bool IsValidSlot(EquipmentSlot slot)
        => Enum.IsDefined(typeof(EquipmentSlot), slot);


}