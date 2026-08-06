using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemDefinition", menuName = "Items/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public string ID;
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
        if (string.IsNullOrEmpty(ID))
            ID = IdFormat(name);

        if (string.IsNullOrEmpty(DisplayName))
            DisplayName = DisplayFormat(name);
    }

    public static string IdFormat(string input)
    {
        return Regex.Replace(input, "([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }

    public static string DisplayFormat(string input)
    {
        return Regex.Replace(input, "([a-z0-9])([A-Z])", "$1 $2");
    }
#endif

}