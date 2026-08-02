using UnityEngine;
[System.Serializable]
public abstract class ItemComponent
{
    protected Item item;

    public virtual void Initialize(Item owner)
    {
        item = owner;
    }

    public virtual void Shutdown()
    {
    }

    public virtual void Activate(IEntity user)
    {
    }
}