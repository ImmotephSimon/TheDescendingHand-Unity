using System.IO;
using UnityEditor;
using UnityEngine;

public class ModifierPoolAssigner : EditorWindow
{
    [MenuItem("Tools/Populate Modifier Pool from JSON")]
    public static void Populate()
    {
        string jsonPath = EditorUtility.OpenFilePanel("Select Pool JSON", "", "json");
        if (string.IsNullOrEmpty(jsonPath)) return;

        // Select target ModifierPool asset in Project Window
        var pool = Selection.activeObject as ModifierPool;
        if (pool == null)
        {
            Debug.LogError("Select your ModifierPool ScriptableObject in the project window first!");
            return;
        }

        string wrappedJson = $"{{\"items\":{File.ReadAllText(jsonPath)}}}";
        var rawData = JsonUtility.FromJson<Wrapper>(wrappedJson);

        Undo.RecordObject(pool, "Populate Modifier Pool");
        pool.Entries.Clear();

        foreach (var raw in rawData.items)
        {
            // Find AffixDefinition SO by matching name
            string[] guids = AssetDatabase.FindAssets($"{raw.Definition} t:AffixDefinition");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"Could not find AffixDefinition named: {raw.Definition}");
                continue;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            var def = AssetDatabase.LoadAssetAtPath<AffixDefinition>(assetPath);

            pool.Entries.Add(new ModifierPoolEntry
            {
                Definition = def,
                Weight = raw.Weight,
                Slot = raw.Slot,
                Modifier = def.Modifier,
                Restriction = def.Restriction
            });
        }

        EditorUtility.SetDirty(pool);
        AssetDatabase.SaveAssets();
        Debug.Log($"Populated {pool.Entries.Count} entries into {pool.name}");
    }

    [System.Serializable]
    private struct RawPoolJson
    {
        public string Definition;
        public float Weight;
        public AffixSlot Slot;
    }

    [System.Serializable]
    private class Wrapper { public RawPoolJson[] items; }
}