public interface IPlayerItemsServerHandler
{
    int InventoryRows { get; }
    int InventoryColumns { get; }

    void HandleInventorySlotClick(int row, int column, bool ctrl);
    void HandleInventorySlotRightClick(int row, int column);
    void HandleUnequip(EquipmentSlot equipmentSlot);
    void HandleInventoryTooltip(int row, int column);
    void HandleEquippedTooltip(EquipmentSlot equipmentSlot);
    void HandleDropHeld();
    void Initialize(PlayerItemsNetwork playerItemsNetwork);
}