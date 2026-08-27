using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class ItemDrop : WorldDrop
{
    private readonly SyncVar<string> _definitionId = new SyncVar<string>();

    private static readonly System.Random Seed = new System.Random();
    private ItemDefinition _definition;
    public ItemInstance Instance { get; set; }

    public ItemDefinition Definition => _definition;

    private void Awake()
    {
        _definitionId.OnChange += OnDefinitionIdChanged;
    }

    public void Initialize(ItemDefinition definition, Rarity rarity)
    {
        int misfortune = Mathf.Clamp(20 - DropLevel / 10, 1, 999);
        Instance = new ItemInstance(
            definition,
            rarity,
            AffixGenerator.Generate(definition, rarity, Seed, misfortune)
        );



        _definitionId.Value = definition.ID;
        ApplyRarityVisualsRpc(rarity.DisplayColor, rarity.LightIntensity, rarity.LightRange);
    }

    private void OnDefinitionIdChanged(string prev, string next, bool asServer)
    {
        if (asServer || (string.IsNullOrEmpty(next))) return;

        if (ItemDatabase.Instance.TryGet(next, out var def))
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
                Debug.LogError($"Item '{def.ID}' has no WorldModel assigned.");
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

    [ObserversRpc]
    private void ApplyRarityVisualsRpc(Color color, float intensity, float range)
    {
        uint layer1 = 1u << 1;

        var light = GetComponentInChildren<Light>();
        if (light != null)
        {
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.renderingLayerMask = (int)layer1;
        }
    }

    protected override bool TryPickup(Player player)
    {
        return player.GetComponent<IInventory>().TryAdd(Instance);
    }
}