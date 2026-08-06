using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Rarity")]
public class Rarity : ScriptableObject
{
    public float DropWeight;
    public int MaxAffixes;
    public int MinAffixes;

    [SerializeReference, SerializeReferenceDropdown]
    public List<LootDefinition> AllowedDrops;
    public bool CanHaveUniqueMods;

    public Color DisplayColor;
    public float LightIntensity = 0.05f; // Default low for Normal
    public float LightRange = 0.05f;
}