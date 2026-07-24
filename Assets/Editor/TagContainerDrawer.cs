using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TagContainer))]
public class TagContainerDrawer : PropertyDrawer
{
    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label)
    {
        string[] availableTags = TagLookup.AvailableTags;

        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty tags =
            property.FindPropertyRelative("Tags");

        EditorGUI.LabelField(
            new Rect(position.x, position.y, position.width, 18),
            label
        );

        float y = position.y + 20;

        for (int i = 0; i < tags.arraySize; i++)
        {
            SerializedProperty element = tags.GetArrayElementAtIndex(i);
            SerializedProperty tagId = element.FindPropertyRelative("TagId");

            int currentIndex = System.Array.IndexOf(availableTags, tagId.stringValue);

            EditorGUI.BeginChangeCheck();

            // If string isn't found in availableTags, EditorGUI.Popup displays -1 as empty/invalid
            int newIndex = EditorGUI.Popup(
                new Rect(position.x, y, position.width - 30, 18),
                currentIndex,
                availableTags
            );

            // ONLY write to stringValue if the user manually changed the selection
            if (EditorGUI.EndChangeCheck() && newIndex >= 0 && newIndex < availableTags.Length)
            {
                tagId.stringValue = availableTags[newIndex];
            }

            if (GUI.Button(new Rect(position.x + position.width - 25, y, 25, 18), "-"))
            {
                tags.DeleteArrayElementAtIndex(i);
            }

            y += 20;
        }

        if (GUI.Button(
            new Rect(position.x, y, position.width, 18),
            "Add Tag"
        ))
        {
            tags.arraySize++;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(
        SerializedProperty property,
        GUIContent label)
    {
        SerializedProperty tags =
            property.FindPropertyRelative("Tags");

        return 40 + tags.arraySize * 20;
    }
}