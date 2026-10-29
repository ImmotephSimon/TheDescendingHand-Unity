using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(VfxType))]
public class VfxStringDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty idProperty = property.FindPropertyRelative("_id");

        if (idProperty == null)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        GameObject currentPrefab = ResolvePrefab(idProperty.stringValue);

        EditorGUI.BeginChangeCheck();

        GameObject selectedPrefab = (GameObject)EditorGUI.ObjectField(
            position, label, currentPrefab, typeof(GameObject), false);

        if (EditorGUI.EndChangeCheck())
        {
            if (selectedPrefab == null)
            {
                idProperty.stringValue = string.Empty;
            }
            else if (!CatalogGenerator.ContainsVfx(selectedPrefab))
            {
                Debug.LogError(
                    $"[VfxString] Prefab '{selectedPrefab.name}' must contain a component implementing IVfx.",
                    selectedPrefab);
            }
            else if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                         selectedPrefab, out string guid, out long localId))
            {
                idProperty.stringValue = $"{guid}:{localId}";
            }
        }

        EditorGUI.EndProperty();
    }

    private static GameObject ResolvePrefab(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        int separator = id.IndexOf(':');

        if (separator < 0)
            return null;

        string guid = id.Substring(0, separator);
        string path = AssetDatabase.GUIDToAssetPath(guid);

        return string.IsNullOrEmpty(path)
            ? null
            : AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }
}