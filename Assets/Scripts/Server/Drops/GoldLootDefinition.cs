using UnityEngine;

public class GoldLootDefinition : LootDefinition
{
    public override void Initialize(WorldDrop drop, Rarity rarity)
    {
        int amount = Mathf.Min(Random.Range(1, 11), Random.Range(1, 11));

        if (drop is GoldDrop goldDrop)
            goldDrop.Initialize(amount);
    }
}
