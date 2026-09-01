using UnityEngine;

[System.Serializable]
public class ItemLootDefinition : LootDefinition
{
    public override float DropHeight => 1f;
    public override float UpForce => 2f;
    public override float ForwardForce => 0.5f;
    public override float Torque => 1.5f;


    public override void Initialize(WorldDrop drop, Rarity rarity)
    {
        if (drop is ItemDrop itemDrop)
        {
            var item = ItemRegistry.Instance.RollRandomItem();
            itemDrop.Initialize(item, rarity);
        }
    }
}