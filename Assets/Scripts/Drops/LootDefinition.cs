using UnityEngine;

[System.Serializable]
public abstract class LootDefinition
{
    public virtual float DropHeight => 0f;
    public virtual float UpForce => 0f;
    public virtual float ForwardForce => 0f;
    public virtual float Torque => 0f;



    public GameObject Prefab;

    public abstract void Initialize(WorldDrop drop, Rarity rarity);
}