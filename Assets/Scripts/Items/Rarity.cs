using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Rarity")]
public class Rarity : ScriptableObject
{
    [SerializeField, HideInInspector]
    private string id;
    public string Id => id;
    public int Tier;
    public float DropWeight;
    public int MaxAffixes;
    public int MinAffixes;

    [SerializeReference, SerializeReferenceDropdown]
    public List<LootDefinition> AllowedDrops;
    public bool CanHaveUniqueMods;

    public Color DisplayColor;
    public float LightIntensity = 0.05f; 
    public float LightRange = 0.05f;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
            id = Guid.NewGuid().ToString();
    }
#endif
}