using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TagContainer))]
public class TagContainerDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty tagsProp = property.FindPropertyRelative("Tags");
        if (tagsProp == null)
        {
            EditorGUI.LabelField(position, label.text, "Error: 'Tags' field not found.");
            EditorGUI.EndProperty();
            return;
        }

        // Draw foldout or standard property field for the list cleanly
        EditorGUI.PropertyField(position, tagsProp, label, true);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty tagsProp = property.FindPropertyRelative("Tags");
        if (tagsProp == null) return EditorGUIUtility.singleLineHeight;

        return EditorGUI.GetPropertyHeight(tagsProp, label, true);
    }
}