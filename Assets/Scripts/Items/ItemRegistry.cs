using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Registries/Items")]
public class ItemRegistry : ScriptableObject
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private List<Rarity> rarities = new();

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

    private Dictionary<string, ItemDefinition> _lookup;

    private void Initialize()
    {
        if (_lookup != null)
            return;

        _lookup = new Dictionary<string, ItemDefinition>();

        foreach (var item in Items)
        {
            if (item != null && !string.IsNullOrEmpty(item.ID))
                _lookup[item.ID] = item;
        }

    }

    public bool TryGet(string id, out ItemDefinition definition)
    {
        Initialize();
        return _lookup.TryGetValue(id, out definition);
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