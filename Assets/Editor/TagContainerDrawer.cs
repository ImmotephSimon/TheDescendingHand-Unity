using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TagContainer))]
public class TagContainerDrawer : PropertyDrawer
{
    private static string[] availableTags;

    private static void EnsureTagsLoaded()
    {
        if (availableTags != null)
            return;

        TextAsset file = AssetDatabase.LoadAssetAtPath<TextAsset>(
            "Assets/Data/Tags.txt"
        );

        if (file == null)
        {
            Debug.LogError("Could not find Tags.txt");
            availableTags = new string[0];
            return;
        }

        availableTags = file.text
            .Split('\n')
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();
    }

    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label)
    {
        EnsureTagsLoaded();

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

            int currentIndex = Mathf.Max(
                0,
                System.Array.IndexOf(
                    availableTags,
                    element.stringValue
                )
            );

            currentIndex = EditorGUI.Popup(
                new Rect(position.x, y, position.width - 30, 18),
                currentIndex,
                availableTags
            );

            element.stringValue = availableTags[currentIndex];

            if (GUI.Button(
                new Rect(position.x + position.width - 25, y, 25, 18),
                "-"
            ))
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