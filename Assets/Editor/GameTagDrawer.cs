using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomPropertyDrawer(typeof(GameTag))]
public class GameTagDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string[] availableTags = TagLookup.AvailableTags;
        SerializedProperty tagId = property.FindPropertyRelative(GameTag.TagIdField);

        if (tagId == null)
        {
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }

        if (availableTags == null || availableTags.Length == 0)
        {
            EditorGUI.PropertyField(position, tagId, label);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        string currentVal = tagId.stringValue;
        GUIContent buttonContent = new GUIContent(string.IsNullOrEmpty(currentVal) ? "Select Tag..." : currentVal);

        // Draw a button field that mimics a dropdown selector
        if (EditorGUI.DropdownButton(position, buttonContent, FocusType.Keyboard))
        {
            var rect = position;
            var dropdown = new GameTagDropdown(new AdvancedDropdownState(), availableTags, selected =>
            {
                tagId.stringValue = selected;
                property.serializedObject.ApplyModifiedProperties();
                GUI.changed = true;
            });
            dropdown.Show(rect);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}