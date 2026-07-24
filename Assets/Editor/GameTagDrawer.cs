using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GameTag))]
public class GameTagDrawer : PropertyDrawer
{
    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label)
    {
        string[] availableTags = TagLookup.AvailableTags;

        SerializedProperty tagId =
            property.FindPropertyRelative("TagId");

        int index = Mathf.Max(
            0,
            System.Array.IndexOf(
                availableTags,
                tagId.stringValue
            )
        );

        EditorGUI.BeginProperty(position, label, property);

        index = EditorGUI.Popup(
            position,
            label.text,
            index,
            availableTags
        );

        if (availableTags.Length > 0)
            tagId.stringValue = availableTags[index];

        EditorGUI.EndProperty();
    }
}