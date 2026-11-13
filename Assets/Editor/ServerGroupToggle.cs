using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class ServerGroupToggle : IPreprocessBuildWithReport
{
    public int callbackOrder => -100;

    public void OnPreprocessBuild(BuildReport report)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var group = settings != null ? settings.FindGroup("Server") : null;
        var schema = group != null ? group.GetSchema<BundledAssetGroupSchema>() : null;
        if (schema == null) return;

        bool isServerBuild =
            EditorUserBuildSettings.standaloneBuildSubtarget == StandaloneBuildSubtarget.Server;

        schema.IncludeInBuild = isServerBuild;
        EditorUtility.SetDirty(schema);
        AssetDatabase.SaveAssets();

        Debug.Log($"[ServerGroupToggle] Server group Include In Build = {isServerBuild}");
    }
}