using UnityEngine;

[System.Serializable]
public abstract class LootDefinition
{
    public GameObject Prefab;

    public abstract void Initialize(WorldDrop drop, Rarity rarity);
}