using System;
using UnityEngine;

public class ItemDrop : WorldDrop
{
    private static readonly System.Random Seed = new System.Random();
    private ItemDefinition _definition;
    private DropNetwork _network;

    public ItemDropInstance Instance { get; set; }

    public ItemDefinition Definition => _definition;

    private void Awake()
    {

        _network = GetComponent<DropNetwork>();

        _network.OnIdChanged += OnIdChanged;
    }

    public void Initialize(ItemDefinition definition, Rarity rarity)
    {
        int misfortune = Mathf.Clamp(20 - DropLevel / 10, 1, 999);
        Instance = new ItemDropInstance(
            definition,
            rarity,
            AffixGenerator.Generate(definition, rarity, Seed, misfortune)
        );

        _network.SetId(definition.Id);
        _network.ApplyRarityVisuals(rarity.DisplayColor, rarity.LightIntensity, rarity.LightRange);
    }

    private void OnIdChanged(Guid guid)
    {
        if (ServerRoot.Instance.ItemRegistry.TryGetDefinition(guid, out var def))
        {
            _definition = def;

            var collider = GetComponent<BoxCollider>();

            if (collider != null)
            {
                collider.size = 0.3f * new Vector3(
                    def.InventorySize.x,
                    def.InventorySize.y,
                    0.5f);
            }

            if (def.Appearance?.WorldModel == null)
            {
                Debug.LogError($"Item '{def.Id}' has no WorldModel assigned.");
                return;
            }
            var spawnedMesh = Instantiate(def.Appearance.WorldModel, transform);

            uint layer0 = 1u << 0;
            uint layer1 = 1u << 1;
            foreach (var renderer in spawnedMesh.GetComponentsInChildren<Renderer>())
            {
                renderer.renderingLayerMask = layer0 | layer1;
            }
        }
    }

    public override bool Restore(IInventoryItem item)
    {
        if (item is not ItemDropInstance instance)
        {
            Debug.LogError($"Invalid restoration type for {item.InventoryId}");
            return false;
        }

        Instance = instance;
        _network.SetId(instance.BaseType.Id);
        _network.ApplyRarityVisuals(instance.Rarity.DisplayColor, instance.Rarity.LightIntensity, instance.Rarity.LightRange);
        return true;
    }



    protected override bool TryPickup(ServerPlayer player)
    {
        return player.GetComponent<IInventory>().TryAdd(Instance);
    }
}