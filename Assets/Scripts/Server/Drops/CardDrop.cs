using UnityEngine;

public class CardDrop : WorldDrop
{
    private CardDefinition _definition;
    private DropNetwork _network;

    public CardItemInstance Instance { get; private set; }

    private void Awake()
    {
        _network = GetComponent<DropNetwork>();
    }

    public void Initialize(CardDefinition definition)
    {
        _definition = definition;
        Instance = new CardItemInstance(definition);

        _network.ApplyRarityVisuals(new Color(0.6f, 0.1f, 1f), 2f, 5f);
    }

    public override bool Restore(IInventoryItem item)
    {
        if (item is not CardItemInstance card)
        {
            Debug.LogError($"Invalid restoration type for {item.InventoryId}");
            return false;
        }

        Instance = card;
        _network.ApplyRarityVisuals(new Color(0.6f, 0.1f, 1f), 2f, 5f);
        return true;
    }

    protected override bool TryPickup(ServerPlayer player)
    {
        return player.GetComponent<IInventory>().TryAdd(Instance);
    }
}