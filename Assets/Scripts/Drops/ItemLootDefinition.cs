[System.Serializable]
public class ItemLootDefinition : LootDefinition
{
    public override void Initialize(WorldDrop drop, Rarity rarity)
    {
        if (drop is ItemDrop itemDrop)
        {
            var item = ItemDatabase.Instance.RollRandomItem();
            itemDrop.Initialize(item, rarity);
        }
    }
}