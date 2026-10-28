[System.Serializable]
public class CardLootDefinition : LootDefinition
{
    public override void Initialize(WorldDrop drop, Rarity rarity)
    {
        if (drop is CardDrop cardDrop)
            cardDrop.Initialize(ItemRegistry.Instance.CardRegistry.GetRandomCard());
    }
}