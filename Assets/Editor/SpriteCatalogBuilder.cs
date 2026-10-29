#if UNITY_EDITOR
using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws SpriteString as a normal Sprite ObjectField. Users select a real
/// Sprite with Unity's visual picker; the serialized value remains an ID.
/// </summary>
[CustomPropertyDrawer(typeof(SpriteString))]
public sealed class SpriteStringDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty idProperty = property.FindPropertyRelative("_id");
        if (idProperty == null)
        {
            EditorGUI.LabelField(position, label.text, "SpriteString is missing its _id field.");
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        string id = idProperty.stringValue;
        Sprite currentSprite = ResolveInEditor(id);
        GUIContent fieldLabel = label;
        if (!string.IsNullOrEmpty(id) && currentSprite == null)
            fieldLabel = new GUIContent(label.text + " (Missing Sprite)", label.tooltip);

        EditorGUI.BeginChangeCheck();
        Sprite selectedSprite = (Sprite)EditorGUI.ObjectField(
            position, fieldLabel, currentSprite, typeof(Sprite), false);

        if (EditorGUI.EndChangeCheck())
        {
            if (selectedSprite == null)
            {
                idProperty.stringValue = string.Empty;
            }
            else if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                         selectedSprite, out string guid, out long localId))
            {
                idProperty.stringValue = MakeId(guid, localId);
            }
            else
            {
                Debug.LogWarning(
                    $"Could not create a stable SpriteString ID for '{selectedSprite.name}'.");
            }
        }

        EditorGUI.EndProperty();
    }

    private static string MakeId(string guid, long localId) =>
        guid + ":" + localId.ToString(CultureInfo.InvariantCulture);

    private static Sprite ResolveInEditor(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        int separator = id.LastIndexOf(':');
        if (separator <= 0 || separator == id.Length - 1)
            return null;

        string guid = id.Substring(0, separator);
        if (!long.TryParse(
                id.Substring(separator + 1), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out long wantedLocalId))
            return null;

        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(path))
            return null;

        foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (!(asset is Sprite sprite))
                continue;

            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                    sprite, out string candidateGuid, out long candidateLocalId) &&
                string.Equals(candidateGuid, guid, StringComparison.Ordinal) &&
                candidateLocalId == wantedLocalId)
                return sprite;
        }

        return null;
    }
}
#endif