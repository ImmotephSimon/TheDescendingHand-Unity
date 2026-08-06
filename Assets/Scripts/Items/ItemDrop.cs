using FishNet.Object.Synchronizing;
using UnityEngine;

public class ItemDrop : WorldDrop
{
    private readonly SyncVar<string> _definitionId = new SyncVar<string>();

    private static readonly System.Random Seed = new System.Random();
    private ItemDefinition _definition;
    private ItemInstance _instance;

    public ItemDefinition Definition => _definition;

    private void Awake()
    {
        _definitionId.OnChange += OnDefinitionIdChanged;
    }

    public override void Initialize(ItemDefinition definition, Rarity rarity)
    {
        base.Initialize(definition, rarity);

        _instance = new ItemInstance(
            definition,
            rarity,
            AffixGenerator.Generate(definition, rarity, Seed)
        );

        _definitionId.Value = definition.ID;
    }

    private void OnDefinitionIdChanged(string prev, string next, bool asServer)
    {
        if (string.IsNullOrEmpty(next)) return;

        if (ItemDatabase.Instance.TryGet(next, out var def))
        {
            _definition = def;
            if (def.Appearance?.WorldModel == null)
            {
                Debug.LogError($"Item '{def.ID}' has no WorldModel assigned.");
                return;
            }
            Instantiate(def.Appearance.WorldModel, transform);
        }
    }

    protected override bool TryPickup(Player player)
    {
        return player.GetComponent<IInventory>().TryAdd(_instance);
    }
}