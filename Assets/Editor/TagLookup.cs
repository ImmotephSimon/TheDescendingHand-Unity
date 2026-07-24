using System.Linq;
using UnityEditor;
using UnityEngine;

public class TagLookup : AssetPostprocessor
{
    private static string[] _availableTags;

    public static string[] AvailableTags
    {
        get
        {
            if (_availableTags == null)
                Load();

            return _availableTags;
        }
    }

    // Automatically called by Unity whenever ANY asset is imported/saved
    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            if (path.EndsWith("Tags.txt"))
            {
                Refresh();
                break;
            }
        }
    }

    public static void Refresh()
    {
        _availableTags = null;
    }

    private static void Load()
    {
        TextAsset file = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Data/Tags.txt");

        if (file == null)
        {
            Debug.LogError("Could not find Tags.txt");
            _availableTags = System.Array.Empty<string>();
            return;
        }

        _availableTags = file.text
            .Split('\n')
            .Select(x => x.Split("//")[0].Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();
    }
}