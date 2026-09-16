#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class AffixBackupEntry
{
    public string name;
    public string id;
    public string nameOverride;
    public List<string> stats;
    public List<string> mathOps;
    public List<float> baseValues;
    public string slot;
}

[System.Serializable]
public class AffixBackupList
{
    public List<AffixBackupEntry> items = new();
}

public static class AffixBackupTool
{
    [MenuItem("Tools/Affixes/Backup to JSON")]
    public static void BackupAffixes()
    {
        string[] guids = AssetDatabase.FindAssets("t:AffixDefinition");
        string folder = "Assets/Data";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string outPath = Path.Combine(folder, $"affixes_backup_{timestamp}.json");

        var backup = new AffixBackupList();

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var def = AssetDatabase.LoadAssetAtPath<AffixDefinition>(path);
            if (def == null) continue;

            backup.items.Add(new AffixBackupEntry
            {
                name = def.name,
                id = def.Id,
                nameOverride = def.NameOverride,
                stats = def.Mods.ConvertAll(m => m.Stat != null ? m.Stat.ToString() : ""),
                mathOps = def.Mods.ConvertAll(m => m.Op.ToString()),
                baseValues = def.Mods.ConvertAll(m => m.Value),
                slot = def.Slot.ToString()
            });
        }

        string json = JsonUtility.ToJson(backup, true);
        File.WriteAllText(outPath, json);
        Debug.Log($"Backed up {backup.items.Count} affixes to {outPath}");
        AssetDatabase.Refresh();
    }
}
#endif