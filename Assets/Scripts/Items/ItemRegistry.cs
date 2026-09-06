using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Registries/Items")]
public class ItemRegistry : ScriptableObject
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private List<Rarity> rarities = new();

    [SerializeField] private CardRegistry cardRegistry;
    [SerializeField] private CardRegistry testCardRegistry;

    public CardRegistry CardRegistry =>
        testCardRegistry != null && testCardRegistry.Cards.Count > 0
            ? testCardRegistry
            : cardRegistry;
    public List<ItemDefinition> Items = new();
    public GameObject ItemPrefab => itemPrefab;
    public List<Rarity> Rarities => rarities;

    private static ItemRegistry _instance;
    public static ItemRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ItemRegistry>("ItemRegistry");
                if (_instance != null)
                {
                    _instance.Initialize();
                }
            }
            return _instance;
        }
    }

    private Dictionary<Guid, ItemDefinition> _lookup;

    private void Initialize()
    {
        if (_lookup != null)
            return;

        _lookup = new Dictionary<Guid, ItemDefinition>();

        foreach (var item in Items)
        {
            if (item == null || item.Id == Guid.Empty)
                continue;

            if (!_lookup.TryAdd(item.Id, item))
            {
                Debug.LogError($"[ItemRegistry] Duplicate ItemDefinition Id '{item.Id}' on '{item.name}'. Collision with '{_lookup[item.Id].name}'.");
            }
        }

    }

    public bool TryGetIcon(Guid id, out Sprite icon)
    {
        Initialize();

        if (_lookup.TryGetValue(id, out var itemDef))
        {
            icon = itemDef.Appearance.Icon;
            return true;
        }

        if (CardRegistry.TryGet(id, out var cardDef))
        {
            icon = cardDef.Visuals.Icon;
            return true;
        }

        Debug.LogError($"Invalid id for TryGetInventoryItem (id: {id})");
        icon = null;
        return false;
    }

    public bool TryGetDefinition(Guid id, out ItemDefinition definition)
    {
        Initialize();
        if (!_lookup.TryGetValue(id, out definition))
        {
            Debug.LogError($"Invalid id for TryGetDefinition (id: {id})");
            return false;
        }

        return true;
    }

    public Rarity RollRandomRarity(System.Random rng, int minimumTier = 0)
    {
        float totalWeight = 0f;

        foreach (var rarity in rarities)
        {
            if (rarity.Tier < minimumTier)
                continue;

            totalWeight += rarity.DropWeight;
        }

        if (totalWeight <= 0f)
            return null;

        double roll = rng.NextDouble() * totalWeight;

        foreach (var rarity in rarities)
        {
            if (rarity.Tier < minimumTier)
                continue;

            roll -= rarity.DropWeight;

            if (roll < 0)
                return rarity;
        }

        return null;
    }

    public bool TryGetRarity(Guid id, out Rarity rarity)
    {
        rarity = rarities.Find(r => r != null && r.Id == id);
        return rarity != null;
    }


    public ItemDefinition RollRandomItem()
    {
        if (Items == null || Items.Count == 0)
        {
            Debug.LogError($"No items set.");
            return null;
        }

        return Items[UnityEngine.Random.Range(0, Items.Count)];
    }
}