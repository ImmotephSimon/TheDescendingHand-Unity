using UnityEditor;

public static class ReserializeTool
{
    [MenuItem("Tools/Reserialize All Assets")]
    public static void Run()
    {
        AssetDatabase.ForceReserializeAssets();
    }
}