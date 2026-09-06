using FishNet.Object;
using UnityEngine;

public class CardDrop : WorldDrop
{
    private CardDefinition _definition;

    public CardItemInstance Instance { get; private set; }


    public void Initialize(CardDefinition definition)
    {
        _definition = definition;
        Instance = new CardItemInstance(definition);

        ApplyRarityVisualsRpc(new Color(0.6f, 0.1f, 1f), 2f, 5f);
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