using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ModifierPoolAssigner : EditorWindow
{
    [MenuItem("Tools/Sync Modifier Pool from Folder")]
    public static void SyncFromFolder()
    {
        var pool = Selection.activeObject as ModifierPool;
        if (pool == null)
        {
            Debug.LogError("Select your ModifierPool ScriptableObject in the project window first!");
            return;
        }

        string poolPath = AssetDatabase.GetAssetPath(pool);
        string parentDir = Path.GetDirectoryName(poolPath);

        string folderPath = EditorUtility.OpenFolderPanel("Select Definitions Folder", parentDir, "");
        if (string.IsNullOrEmpty(folderPath)) return;

        if (!folderPath.StartsWith(Application.dataPath))
        {
            Debug.LogError("Selected folder must be inside the project's Assets folder.");
            return;
        }
        string relativePath = "Assets" + folderPath.Substring(Application.dataPath.Length);

        // Gather all valid AffixDefinitions in target directory
        string[] guids = AssetDatabase.FindAssets("t:AffixDefinition", new string[] { relativePath });
        var folderDefs = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<AffixDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(def => def != null)
            .ToHashSet();

        Undo.RecordObject(pool, "Sync Modifier Pool");

        // 1. Prune missing/deleted assets
        pool.Entries.RemoveAll(entry => entry.Definition == null || !folderDefs.Contains(entry.Definition));

        // 2. Find currently tracked definitions to avoid resetting existing entries
        var existingDefs = pool.Entries.Select(e => e.Definition).ToHashSet();

        // 3. Append missing definitions with default values
        foreach (var def in folderDefs)
        {
            if (existingDefs.Contains(def)) continue;

            pool.Entries.Add(new ModifierPoolEntry(def, 1f));
        }

        EditorUtility.SetDirty(pool);
        AssetDatabase.SaveAssets();
        Debug.Log($"Synced {pool.name}: {pool.Entries.Count} total entries.");
    }
}