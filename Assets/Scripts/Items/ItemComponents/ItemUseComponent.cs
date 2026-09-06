using UnityEngine;
[System.Serializable]
public abstract class ItemUseComponent
{
    public virtual void Initialize(ItemDropInstance Instance)
    {
    }

    public abstract void Use(ItemDropInstance instance, IEntity user);
}