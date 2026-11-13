using UnityEditor;
using UnityEngine;

public static class FindServerLeaks
{
    private const string ServerAssembly = "Game.Server"; // the asmdef's name

    [MenuItem("Tools/Find Server Components Reachable From Main")]
    private static void Run()
    {
        var deps = AssetDatabase.GetDependencies("Assets/Scenes/Main.unity", true);

        foreach (var path in deps)
        {
            if (!path.EndsWith(".prefab")) continue;

            var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null) continue;
                if (mb.GetType().Assembly.GetName().Name == ServerAssembly)
                    Debug.LogWarning($"{mb.GetType().Name} on '{mb.name}' in {path}", root);
            }
        }
    }
}