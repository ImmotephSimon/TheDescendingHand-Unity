using FishNet.Managing.Object;
using FishNet.Object;
using GameKit.Dependencies.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Scans the project for every prefab with a NetworkObject on its root and builds:
///   ClientCollection: shared prefabs + client prefabs (slot order = sorted asset path)
///   ServerCollection: copy of the client list, with each Dual pair's server prefab in its client twin's slot,
///                     and unpaired server-only prefabs appended at the end.
/// Every prefab's stored PrefabId must equal its slot index; the script verifies that.
/// </summary>
public static class SplitPrefabCollections
{
    private const string ServerAssembly = "Game.Server";

    // Prefab names to leave out of both lists (e.g. variant parents that are never spawned).
    private static readonly HashSet<string> ExcludedNames = new HashSet<string>
    {
        // "ClientBaseDrop",
        // "ClientRoomBase",
    };

    [MenuItem("Tools/Prefabs/Split Prefab Collections")]
    private static void Run()
    {
        var registry = FindSingle<ServerPrefabRegistry>();
        if (registry == null)
        {
            Debug.LogError("Need exactly one ServerPrefabRegistry asset in the project.");
            return;
        }

        if (registry.ClientCollection == null || registry.ServerCollection == null)
        {
            Debug.LogError("Assign ClientCollection and ServerCollection on the ServerPrefabRegistry first.");
            return;
        }

        // 1. Dual pairs (server prefab -> client twin).
        var pairs = new List<DualPrefab>();
        foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject"))
        {
            var dual = AssetDatabase.LoadAssetAtPath<DualPrefabObjects>(AssetDatabase.GUIDToAssetPath(guid));
            if (dual == null) continue;
            foreach (var pair in dual.Prefabs)
                if (pair.Client != null && pair.Server != null)
                    pairs.Add(pair);
        }

        var pairedServers = new HashSet<NetworkObject>(pairs.Select(p => p.Server));
        var pairedClients = new HashSet<NetworkObject>(pairs.Select(p => p.Client));

        // 2. Scan the whole project for prefabs with a NetworkObject on the root.
        var all = new List<NetworkObject>();
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.StartsWith("Assets/")) continue;

            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null || ExcludedNames.Contains(go.name)) continue;
            if (!go.TryGetComponent<NetworkObject>(out var nob)) continue;

            all.Add(nob);
        }
        SetAssetPathHashes(all);
        all = all.OrderBy(n => n.AssetPathHash).ToList();

        // 3. Client list: everything that isn't a server prefab, plus every Dual client twin.
        var client = new List<NetworkObject>();
        foreach (var nob in all)
        {
            if (pairedServers.Contains(nob)) continue;
            if (IsServerOnly(nob.gameObject) && !pairedClients.Contains(nob)) continue;
            client.Add(nob);
        }
        foreach (var c in pairedClients)
            if (!client.Contains(c))
                client.Add(c);

        // 4. Server list: copy of client list, server variants swapped into their twin's slot.
        var server = new List<NetworkObject>(client);
        foreach (var pair in pairs)
        {
            int slot = server.IndexOf(pair.Client);
            if (slot >= 0)
                server[slot] = pair.Server;
        }

        // 5. Server-only prefabs without a client twin go on the end.
        foreach (var nob in all)
        {
            if (server.Contains(nob) || pairedServers.Contains(nob)) continue;
            if (!IsServerOnly(nob.gameObject)) continue;
            server.Add(nob);
        }

        // 6. Wipe old ids on every scanned prefab, then fill the collections and stamp ids.
        ResetIds(all);

        registry.ClientCollection.Clear();
        registry.ClientCollection.AddObjects(client, checkForDuplicates: true, initializeAdded: true);
        EditorUtility.SetDirty(registry.ClientCollection);

        registry.ServerCollection.Clear();
        registry.ServerCollection.AddObjects(server, checkForDuplicates: false, initializeAdded: true);
        EditorUtility.SetDirty(registry.ServerCollection);

        // 7. If AddObjects didn't write the ids, write them ourselves.
        if (CountMismatches(client) + CountMismatches(server) > 0)
        {
            StampIds(client);
            StampIds(server);

            int clientAfter = CountMismatches(client);
            int serverAfter = CountMismatches(server);

            Debug.Log($"After stamping: Client={clientAfter}, Server={serverAfter}");
        }

        // 8. Save the prefab assets so the ids survive entering play mode.
        foreach (var nob in server.Concat(client).Distinct())
        {
            if (nob == null) continue;
            EditorUtility.SetDirty(nob);
            EditorUtility.SetDirty(nob.gameObject);
            PrefabUtility.SavePrefabAsset(nob.gameObject);
        }
        AssetDatabase.SaveAssets();

        // 9. Verify.
        int bad = CheckIds("Client", client) + CheckIds("Server", server);
        Debug.Log($"Split done. Scanned {all.Count} prefabs with NetworkObject. " +
                  $"Client: {client.Count}, Server: {server.Count}, pairs: {pairs.Count}, mismatches: {bad}.");
    }

    [MenuItem("Tools/Prefabs/Dump Collection Ids")]
    private static void Dump()
    {
        var registry = FindSingle<ServerPrefabRegistry>();
        if (registry == null) return;

        DumpCollection("Server", registry.ServerCollection, true);
        DumpCollection("Client", registry.ClientCollection, false);
    }

    private static void DumpCollection(string label, PrefabObjects collection, bool asServer)
    {
        if (collection == null) return;
        for (int i = 0; i < collection.GetObjectCount(); i++)
        {
            var o = collection.GetObject(asServer, i);
            string state = o == null ? "NULL" : (o.PrefabId == i ? "ok" : "MISMATCH");
            Debug.Log($"[{label}] index={i} {(o == null ? "null" : o.name)} storedId={(o == null ? -1 : o.PrefabId)} {state}");
        }
    }

    private static int CountMismatches(List<NetworkObject> list)
    {
        int bad = 0;
        for (int i = 0; i < list.Count; i++)
            if (list[i] != null && list[i].PrefabId != i)
                bad++;
        return bad;
    }

    private static int CheckIds(string label, List<NetworkObject> list)
    {
        int bad = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null || list[i].PrefabId == i) continue;
            bad++;
            Debug.LogWarning($"[{label}] MISMATCH index={i} {list[i].name} storedId={list[i].PrefabId}");
        }
        return bad;
    }

    // Writes the id straight into the serialized field. The field name depends on the FishNet version,
    // so a few likely names are tried.
    private static void StampIds(List<NetworkObject> list)
    {
        string[] names = { "_prefabId", "<PrefabId>k__BackingField", "PrefabId" };

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null) continue;

            var so = new SerializedObject(list[i]);
            SerializedProperty prop = null;
            foreach (var n in names)
            {
                prop = so.FindProperty(n);
                if (prop != null) break;
            }

            if (prop == null)
            {
                Debug.LogError("Could not find the PrefabId field on NetworkObject. " +
                               "Open a prefab in Inspector debug mode and look for the id field name.");
                return;
            }

            prop.intValue = i;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(list[i]);
        }
    }

    // Sets every prefab's id back to "unassigned" (65535) so no old id can survive the rebuild.
    private static void ResetIds(List<NetworkObject> list)
    {
        string[] names = { "_prefabId", "<PrefabId>k__BackingField", "PrefabId" };

        foreach (var nob in list)
        {
            if (nob == null) continue;

            var so = new SerializedObject(nob);
            SerializedProperty prop = null;
            foreach (var n in names)
            {
                prop = so.FindProperty(n);
                if (prop != null) break;
            }

            if (prop == null)
            {
                Debug.LogWarning("ResetIds: could not find the PrefabId field, skipping the reset.");
                return;
            }

            prop.intValue = ushort.MaxValue;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(nob);
        }
    }

    private static bool IsServerOnly(GameObject go)
    {
        if (go.name.StartsWith("Server")) return true;

        foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb != null && mb.GetType().Assembly.GetName().Name == ServerAssembly)
                return true;
        }
        return false;
    }

    private static T FindSingle<T>() where T : Object
    {
        var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        if (guids.Length != 1) return null;
        return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    private static void SetAssetPathHashes(List<NetworkObject> prefabs)
    {
        foreach (var nob in prefabs)
        {
            string pathAndName =
                $"{AssetDatabase.GetAssetPath(nob.gameObject)}{nob.gameObject.name}"
                .Trim()
                .ToLowerInvariant();

            var sb = new System.Text.StringBuilder();

            foreach (char c in pathAndName)
            {
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                    sb.Append(c);
            }

            ulong hash = sb.ToString().GetStableHashU64();
            nob.SetAssetPathHash(hash);
            EditorUtility.SetDirty(nob);
        }
    }
}