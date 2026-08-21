using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private List<Rarity> rarities = new();

    public List<ItemDefinition> Items = new();
    public GameObject ItemPrefab => itemPrefab;
    public List<Rarity> Rarities => rarities;

    private static ItemDatabase _instance;
    public static ItemDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ItemDatabase>("ItemDatabase");
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

    public Rarity RollRandomRarity(System.Random rng)
    {
        if (rarities == null || rarities.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var rarity in rarities)
            totalWeight += rarity.DropWeight;

        if (totalWeight <= 0f) return rarities[0];

        double roll = rng.NextDouble() * totalWeight;
        float currentWeight = 0f;

        for (int i = 0; i < rarities.Count; i++)
        {
            currentWeight += rarities[i].DropWeight;
            if (roll < currentWeight)
                return rarities[i];
        }

        return rarities[0];
    }

    public ItemDefinition RollRandomItem()
    {
        if (Items == null || Items.Count == 0)
            return null;

        return Items[Random.Range(0, Items.Count)];
    }
}