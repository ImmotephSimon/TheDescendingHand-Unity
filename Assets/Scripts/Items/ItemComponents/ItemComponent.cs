using UnityEngine;

public abstract class ItemComponent : ScriptableObject
{
    protected Item item;

    public virtual void Initialize(Item owner)
    {
        item = owner;
    }

    public virtual void Shutdown()
    {
    }

    public virtual void Activate(Character user)
    {
    }
}