
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class SpriteReferenceAudit
{
    [MenuItem("Tools/Sprites/Audit ScriptableObject Sprite References")]
    private static void Audit()
    {
        int count = 0;

        foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadMainAssetAtPath(path);

            if (asset == null)
                continue;

            var serialized = new SerializedObject(asset);
            var property = serialized.GetIterator();

            while (property.Next(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                if (!(property.objectReferenceValue is Sprite sprite))
                    continue;

                string spritePath = AssetDatabase.GetAssetPath(sprite);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                    sprite, out string spriteGuid, out long localId);

                Debug.Log(
                    $"[Sprite Audit] Asset: {path} | Field: {property.propertyPath} | " +
                    $"Sprite: {sprite.name} | Sprite ID: {spriteGuid}:{localId} | " +
                    $"Sprite Asset: {spritePath}",
                    asset);

                count++;
            }
        }

        Debug.Log($"Sprite audit complete. Found {count} serialized Sprite references.");
    }
}
#endif
