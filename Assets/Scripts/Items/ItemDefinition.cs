using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class ItemDefinition : ScriptableObject
{
    public string ID;
    public string DisplayName;
    public ItemRarity Rarity;

    public AppearanceData Appearance;
    public int RequiredLevel;

    public List<ImplicitData> Implicits;

    public List<ItemComponent> Components;

    public TagContainer Tags;

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