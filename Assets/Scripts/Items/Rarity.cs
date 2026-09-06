using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Rarity")]
public class Rarity : ScriptableObject
{
    [SerializeField, HideInInspector]
    private Guid id;
    public Guid Id => id;
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
        string path = UnityEditor.AssetDatabase.GetAssetPath(this);
        if (!string.IsNullOrEmpty(path))
        {
            string hex = UnityEditor.AssetDatabase.AssetPathToGUID(path);
            id = Guid.Parse(hex);
        }
    }
#endif
}