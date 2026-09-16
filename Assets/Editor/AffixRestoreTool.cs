#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class AffixRestoreEntry
{
    public string name;
    public string id;
    public float baseValue;
    public string modifierTag;
    public string mathOp;
    public string restrictionTag;
    public string slot;
}

[System.Serializable]
public class AffixRestoreArrayWrapper
{
    public List<AffixRestoreEntry> items = new();
}

public static class AffixRestoreTool
{
    [MenuItem("Tools/Affixes/Restore From Backup JSON")]
    public static void Restore()
    {
        string path = EditorUtility.OpenFilePanel("Select Affix Backup JSON", "Assets/Data", "json");
        if (string.IsNullOrEmpty(path)) return;

        string rawJson = File.ReadAllText(path).Trim();
        if (string.IsNullOrEmpty(rawJson)) return;

        var backup = JsonUtility.FromJson<AffixRestoreArrayWrapper>(rawJson);
        if (backup == null || backup.items == null || backup.items.Count == 0)
        {
            Debug.LogError("Failed to deserialize JSON content.");
            return;
        }

        var guids = AssetDatabase.FindAssets("t:AffixDefinition");
        var defsById = new Dictionary<string, AffixDefinition>(StringComparer.OrdinalIgnoreCase);
        var defsByName = new Dictionary<string, AffixDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (var g in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(g);
            var d = AssetDatabase.LoadAssetAtPath<AffixDefinition>(assetPath);
            if (d == null) continue;

            if (!string.IsNullOrEmpty(d.Id))
            {
                defsById.TryAdd(d.Id, d);
            }
            defsByName.TryAdd(d.name, d);
        }

        int restored = 0;
        foreach (var entry in backup.items)
        {
            AffixDefinition def = null;

            // 1. Try matching by GUID Id
            if (!string.IsNullOrEmpty(entry.id) && !defsById.TryGetValue(entry.id, out def))
            {
                // 2. Fall back to asset file name
                defsByName.TryGetValue(entry.name, out def);
            }

            if (def == null)
            {
                Debug.LogWarning($"No matching asset found for Id '{entry.id}' or Name '{entry.name}'");
                continue;
            }

            if (!Enum.TryParse<MathOp>(entry.mathOp, out var op))
            {
                Debug.LogWarning($"Failed to parse MathOp '{entry.mathOp}' for '{entry.name}'");
                continue;
            }

            // Assign AffixSlot enum if valid
            if (!string.IsNullOrEmpty(entry.slot))
            {
                if (Enum.TryParse<AffixSlot>(entry.slot, true, out var parsedSlot))
                {
                    def.Slot = parsedSlot;
                }
                else
                {
                    Debug.LogWarning($"Slot value '{entry.slot}' on '{entry.name}' does not map to AffixSlot enum. Skipping slot assignment.");
                }
            }

            var modTag = string.IsNullOrEmpty(entry.modifierTag) ? GameTag.Empty : new GameTag(entry.modifierTag);

            TagRequirement req = TagRequirement.Empty;
            if (!string.IsNullOrEmpty(entry.restrictionTag))
            {
                var container = new TagContainer(new GameTag(entry.restrictionTag));
                req = new TagRequirement(container);
            }

            def.Mods = new List<StatModifier>
            {
                new StatModifier(modTag, op, entry.baseValue, req)
            };

            EditorUtility.SetDirty(def);
            restored++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Restored {restored} of {backup.items.Count} affix definitions.");
    }
}
#endif