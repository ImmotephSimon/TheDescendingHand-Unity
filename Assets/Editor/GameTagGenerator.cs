using System.IO;
using UnityEditor;
using UnityEngine;

public static class GameTagGenerator
{
    private const string TagFile = "Assets/Data/Tags.txt";
    private const string OutputFile = "Assets/Scripts/Generated/GameTags.cs";

    [MenuItem("Tools/Generate GameTags")]
    public static void Generate()
    {
        if (!File.Exists(TagFile))
        {
            Debug.LogError($"Missing tag file: {TagFile}");
            return;
        }

        var lines = File.ReadAllLines(TagFile);

        using (var writer = new StreamWriter(OutputFile))
        {
            writer.WriteLine("// Auto-generated. Do not edit.");
            writer.WriteLine();
            writer.WriteLine("public static class GameTags");
            writer.WriteLine("{");

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string propertyName = ToIdentifier(line);

                writer.WriteLine(
                    $"    public static readonly GameTag {propertyName} = new(\"{line}\");");
            }

            writer.WriteLine("}");
        }

        

        AssetDatabase.Refresh();

        Debug.Log("Generated GameTags.cs");
    }

    private static string ToIdentifier(string tag)
    {
        var parts = tag.Split('.');

        return string.Concat(parts);
    }

    private class TagPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var asset in importedAssets)
            {
                if (asset == OutputFile)
                    continue;

                if (asset == TagFile)
                {
                    EditorApplication.delayCall += Generate;
                    break;
                }
            }
        }
    }
}