using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemDefinition> Items = new();

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
        if (_lookup != null) return;

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
}