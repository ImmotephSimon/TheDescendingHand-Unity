using UnityEditor;
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
            Debug.LogError(
                $"GameTagDrawer: Could not find serialized field '{GameTag.TagIdField}' " +
                $"in '{property.propertyPath}'.",
                property.serializedObject.targetObject);
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }
        if (availableTags == null || availableTags.Length == 0)
        {
            EditorGUI.PropertyField(position, tagId, label);
            return;
        }

        int currentIndex = System.Array.IndexOf(availableTags, tagId.stringValue);

        EditorGUI.BeginProperty(position, label, property);
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            // If tag wasn't found, display index 0 visually, but don't commit it to the property yet
            int selectedIndex = EditorGUI.Popup(
                position,
                label.text,
                currentIndex < 0 ? 0 : currentIndex,
                availableTags
            );

            // Only update the actual serialized value when the user actively picks a new option
            if (check.changed && selectedIndex >= 0 && selectedIndex < availableTags.Length)
            {
                tagId.stringValue = availableTags[selectedIndex];
            }
        }
        EditorGUI.EndProperty();
    }
}