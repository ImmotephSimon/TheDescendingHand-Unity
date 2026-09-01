// Assets/Editor/ItemDatabaseBuilder.cs

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class ItemDatabaseBuilder
{
    private const string DatabasePath = "Assets/Data/ItemDatabase.asset";

    [MenuItem("Tools/Items/Rebuild Database")]
    public static void Rebuild()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemDefinition");

        List<ItemDefinition> definitions = new();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemDefinition definition = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);

            if (definition != null)
                definitions.Add(definition);
        }

        ItemRegistry database = AssetDatabase.LoadAssetAtPath<ItemRegistry>(DatabasePath);

        if (database == null)
        {
            database = ScriptableObject.CreateInstance<ItemRegistry>();
            AssetDatabase.CreateAsset(database, DatabasePath);
        }

        database.Items = definitions;

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();

        Debug.Log($"Rebuilt ItemDatabase with {definitions.Count} items.");
    }
}