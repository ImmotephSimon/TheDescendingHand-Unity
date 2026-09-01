using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class PlayerItemsSync : NetworkBehaviour
{

    [Serializable]
    public struct InventoryItemDto
    {
        public string ItemId;
        public string BaseTypeId;
        public string RarityId;
        public Vector2Int Position;
        public Vector2Int Size;
        public List<AffixState> Explicits;
    }

    [Serializable]
    public struct EquippedItemDto
    {
        public string EquipmentTypeId;
        public string BaseTypeId;
        public string RarityId;
        public List<AffixState> Explicits;
    }

    [Serializable]
    public struct AffixState
    {
        public string DefinitionId;
        public float Tier;
    }

    private readonly SyncList<InventoryItemDto> _inventoryItems = new();
    private PlayerInventory _inventory;
    private Loadout _loadout;
    private readonly SyncList<EquippedItemDto> _equippedItems = new();

    public event Action InventoryChanged;
    public event Action LoadoutChanged;

    private readonly SyncVar<int> _inventoryRows = new(4);
    private readonly SyncVar<int> _inventoryColumns = new(4);

    public int InventoryRows => _inventoryRows.Value;
    public int InventoryColumns => _inventoryColumns.Value;
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

        foreach (var entry in _inventory.GetPlacedItems())
        {
            var item = (ItemInstance)entry.Key;

            _inventoryItems.Add(new InventoryItemDto
            {
                ItemId = item.Id.ToString(),
                BaseTypeId = item.BaseType.ID,
                RarityId = item.Rarity.Id,
                Position = entry.Value,
                Size = item.Size,
                Explicits = item.Explicits
                    .Select(affix => new AffixState
                    {
                        DefinitionId = affix.Definition.Id,
                        Tier = affix.Tier
                    })
                    .ToList()
            });
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

    public IReadOnlyDictionary<IInventoryItem, Vector2Int> GetInventoryItems()
    {
        var result = new Dictionary<IInventoryItem, Vector2Int>();

        foreach (var dto in _inventoryItems)
        {
            var item = ReconstructItem(dto);
            if (item != null)
                result[item] = dto.Position;
        }

        return result;
    }

    public bool TryGetInventoryItem(
        int row,
        int column,
        out IInventoryItem item)
    {
        foreach (InventoryItemDto dto in _inventoryItems)
        {
            Vector2Int origin = dto.Position;
            Vector2Int size = dto.Size;

            if (column < origin.x ||
                column >= origin.x + size.x ||
                row < origin.y ||
                row >= origin.y + size.y)
            {
                continue;
            }

            item = ReconstructItem(dto);
            return item != null;
        }

        item = null;
        return false;
    }

    public ItemInstance ReconstructItem(InventoryItemDto dto)
    {
        return ReconstructItem(dto.BaseTypeId, dto.RarityId, dto.Explicits);
    }

    public ItemInstance ReconstructItem(EquippedItemDto dto)
    {
        return ReconstructItem(dto.BaseTypeId, dto.RarityId, dto.Explicits);
    }

    private ItemInstance ReconstructItem(
        string baseTypeId,
        string rarityId,
        List<AffixState> explicits)
    {
        if (!ItemRegistry.Instance.TryGet(baseTypeId, out var baseType))
            return null;

        var equip = baseType.Components
            .OfType<EquipComponentDefinition>()
            .FirstOrDefault();

        if (equip.EquipmentType.ModifierPool.Entries == null)
            return null;

        var affixes = explicits
            .Select(state =>
            {
                var entry = equip.EquipmentType.ModifierPool.Entries
                    .FirstOrDefault(x =>
                        x.Definition != null &&
                        x.Definition.Id == state.DefinitionId);

                if (entry?.Definition == null)
                    return null;

                return new AffixInstance
                {
                    Definition = entry.Definition,
                    Tier = state.Tier
                };
            })
            .Where(x => x != null)
            .ToList();

        var rarity = ItemRegistry.Instance.Rarities
            .FirstOrDefault(x => x.Id == rarityId);

        return new ItemInstance(baseType, rarity, affixes);
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
                var item = entry.Value;

                _equippedItems.Add(new EquippedItemDto
                {
                    EquipmentTypeId = entry.Key.ID,
                    BaseTypeId = item.BaseType.ID,
                    RarityId = item.Rarity.Id,
                    Explicits = item.Explicits
                        .Select(affix => new AffixState
                        {
                            DefinitionId = affix.Definition.Id,
                            Tier = affix.Tier
                        })
                        .ToList()
                });
            }
        }

        LoadoutChanged?.Invoke();
    }


    [ServerRpc]
    public void RequestUnequip(string equipmentTypeId)
    {
        Debug.Log($"Unequip request: {equipmentTypeId}");
        foreach (var entry in _loadout.Equipped)
            Debug.Log($"Equipped: {entry.Key} -> {entry.Value.BaseType.name}");

        EquipmentType.TryGet(equipmentTypeId, out EquipmentType equipmentSlot);
        _loadout.Unequip(equipmentSlot);
    }
}