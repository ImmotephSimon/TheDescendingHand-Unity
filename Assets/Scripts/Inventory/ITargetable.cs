using System;

public enum InventoryResponse
{
    Failed,
    Consumed
}

public interface ITargetable
{
    TagContainer GetTargetingRequirements();
    InventoryResponse ApplyTargetedEffect(ItemInstance targetItem);
    void StartTargeting(ItemInstance orbInstance, TagContainer requirements, Action<ItemInstance> onTargetSelected);
}