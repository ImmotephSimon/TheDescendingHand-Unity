
using System.Linq;
using UnityEditor;

public static class ReserializeTool
{
    [MenuItem("Tools/Reserialize All ScriptableObjects")]
    public static void Run()
    {
        string[] assets = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .ToArray();

        AssetDatabase.ForceReserializeAssets(assets);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
