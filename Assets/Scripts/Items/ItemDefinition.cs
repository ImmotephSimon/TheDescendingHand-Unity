using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemDefinition", menuName = "Items/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public Guid Id;
    public string DisplayName;

    
    public Vector2Int InventorySize = new Vector2Int(2,2);
    public AppearanceData Appearance;
    public int RequiredLevel;
    public string Lore;
    public TagContainer Tags;
    public List<StatModifier> Implicits;

    [SerializeReference, SerializeReferenceDropdown]
    public List<ItemComponentDefinition> Components = new();



#if UNITY_EDITOR
    private void OnValidate()
    {
        string path = UnityEditor.AssetDatabase.GetAssetPath(this);
        if (!string.IsNullOrEmpty(path))
        {
            string hex = UnityEditor.AssetDatabase.AssetPathToGUID(path);
            Id = System.Guid.Parse(hex);
        }

        if (string.IsNullOrEmpty(DisplayName))
            DisplayName = DisplayFormat(name);
    }

    public static string DisplayFormat(string input)
    {
        return Regex.Replace(input, "([a-z0-9])([A-Z])", "$1 $2");
    }
#endif

}