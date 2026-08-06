using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class DropTableEntry
{
    public Rarity Rarity;
    public float Weight;

    [SerializeReference, SerializeReferenceDropdown]
    public LootDefinition Loot;
}