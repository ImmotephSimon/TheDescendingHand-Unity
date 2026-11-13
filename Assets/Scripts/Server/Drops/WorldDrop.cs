using System;
using UnityEngine;

public abstract class WorldDrop : MonoBehaviour, IInteractable
{
    public int DropLevel { get; set; } = 1;

    public event Action<WorldDrop> Consumed;

    private bool _pickedUp;

    protected bool CanBePickedUp => !_pickedUp;

    protected void Consume()
    {
        _pickedUp = true;
        Consumed?.Invoke(this);
    }

    public void Interact(ServerPlayer player)
    {
        if (CanBePickedUp && TryPickup(player))
            Consume();
    }

    public virtual bool Restore(IInventoryItem item) => false;

    protected virtual bool TryPickup(ServerPlayer player) => false;
}