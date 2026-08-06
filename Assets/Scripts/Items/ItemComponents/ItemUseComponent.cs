using UnityEngine;
[System.Serializable]
public abstract class ItemUseComponent
{
    public virtual void Initialize(ItemInstance Instance)
    {
    }

    public abstract void Use(ItemInstance instance, IEntity user);
}