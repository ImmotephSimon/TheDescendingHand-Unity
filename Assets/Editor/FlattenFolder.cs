using System.IO;
using UnityEditor;
using UnityEngine;

public class FlattenFolder
{
    [MenuItem("Tools/Flatten Folder PNGs")]
    public static void Flatten()
    {
        string rootPath = "Assets/Imports/Blink";

        if (!Directory.Exists(rootPath))
        {
            Debug.LogError($"Directory not found: {rootPath}");
            return;
        }

        // Get full system path to ensure accurate directory comparison
        string fullRootPath = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string[] files = Directory.GetFiles(rootPath, "*.png", SearchOption.AllDirectories);

        int movedCount = 0;

        AssetDatabase.StartAssetEditing();

        foreach (string file in files)
        {
            // Normalize path separators to forward slashes for Unity's AssetDatabase
            string normalizedFile = file.Replace('\\', '/');
            string fileDir = Path.GetFullPath(Path.GetDirectoryName(normalizedFile)).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Skip files that are already directly in the root target folder
            if (string.Equals(fileDir, fullRootPath, System.StringComparison.OrdinalIgnoreCase))
                continue;

            string fileName = Path.GetFileName(normalizedFile);
            string destFile = $"{rootPath}/{fileName}";

            // Ensure unique path if a file with the same name already exists in root
            destFile = AssetDatabase.GenerateUniqueAssetPath(destFile);

            string result = AssetDatabase.MoveAsset(normalizedFile, destFile);
            if (string.IsNullOrEmpty(result))
            {
                movedCount++;
            }
            else
            {
                Debug.LogWarning($"Failed to move {normalizedFile}: {result}");
            }
        }

        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();

        Debug.Log($"Successfully flattened {movedCount} PNGs into {rootPath}");
    }
}