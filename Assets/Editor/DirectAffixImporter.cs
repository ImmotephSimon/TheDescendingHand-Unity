using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using static Codice.Client.BaseCommands.Import.Commit;

public class AffixDefinitionImporter : EditorWindow
{
    [MenuItem("Tools/Import Affix Definitions Direct")]
    public static void Import()
    {
        string path = EditorUtility.OpenFilePanel("Select Definitions JSON", "", "json");
        if (string.IsNullOrEmpty(path)) return;

        string wrappedJson = $"{{\"items\":{File.ReadAllText(path)}}}";
        var rawData = JsonUtility.FromJson<Wrapper>(wrappedJson);

        string folder = "Assets/Data/Affixes";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/Data", "Affixes");

        var validTagIds = typeof(GameTags)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(GameTag))
            .Select(f => f.GetValue(null) as GameTag)
            .Where(gt => gt != null && !string.IsNullOrEmpty(gt.TagId))
            .Select(gt => gt.TagId)
            .ToHashSet();

        // Fallback to force static initialization if fields were null
        if (validTagIds.Count == 0)
        {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(GameTags).TypeHandle);

            validTagIds = typeof(GameTags)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(GameTag))
                .Select(f => (f.GetValue(null) as GameTag)?.TagId)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToHashSet();
        }
        foreach (var raw in rawData.items)
        {
            var asset = ScriptableObject.CreateInstance<AffixDefinition>();
            asset.name = raw.Id;
            asset.NameOverride = raw.Id;
            asset.BaseValue = raw.BaseValue;
            string tagId = validTagIds.Contains(raw.Modifier) ? raw.Modifier : string.Empty;
            if (System.Enum.TryParse<MathOp>(raw.MathOp, out var parsedOp)) asset.MathOp = parsedOp;
            else Debug.LogWarning($"Failed to parse MathOp '{raw.MathOp}' for {raw.Id}");
            asset.Modifier = new GameTag(tagId);

            AssetDatabase.CreateAsset(asset, $"{folder}/{raw.Id}.asset");
            EditorUtility.SetDirty(asset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [System.Serializable]
    private class Wrapper { public RawDefinitionJson[] items; }
}