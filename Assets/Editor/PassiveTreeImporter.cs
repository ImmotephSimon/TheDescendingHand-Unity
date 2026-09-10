#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class PassiveTreeImporter
{
    private const string DataPath = "Assets/Data/PassiveTree";
    private const string NodesPath = DataPath + "/PassiveTreeNodes.json";
    private const string TypesPath = DataPath + "/NodeTypes.json";
    private const string NeighborsPath = DataPath + "/NodeNeighbors.json";
    private const string OutputPath = DataPath + "/PassiveTreeData.asset";

    [MenuItem("Tools/Passive Tree/Import")]
    public static void Import()
    {
        string nodesJson = File.ReadAllText(NodesPath);
        string typesJson = File.ReadAllText(TypesPath);
        string neighborsJson = File.ReadAllText(NeighborsPath);

        PassiveNodeJson[] nodes = JsonUtility.FromJson<PassiveNodeJsonArray>(
            "{\"Items\":" + nodesJson + "}").Items;

        PassiveNodeTypeJson[] types = JsonUtility.FromJson<PassiveNodeTypeJsonArray>(
            "{\"Items\":" + typesJson + "}").Items;

        PassiveNodeNeighborsJson[] neighbors = JsonUtility.FromJson<PassiveNodeNeighborsJsonArray>(
            "{\"Items\":" + neighborsJson + "}").Items;

        PassiveTreeData tree = LoadOrCreateTree();

        tree.Nodes.Clear();
        tree.NodeTypes.Clear();
        tree.Connections.Clear();

        ImportNodes(tree, nodes);
        ImportNodeTypes(tree, types);
        ImportConnections(tree, neighbors);

        EditorUtility.SetDirty(tree);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"Imported passive tree: {tree.Nodes.Count} nodes, " +
            $"{tree.NodeTypes.Count} types, " +
            $"{tree.Connections.Count} connections.");
    }

    private static PassiveTreeData LoadOrCreateTree()
    {
        string directory = Path.GetDirectoryName(OutputPath);

        if (!AssetDatabase.IsValidFolder(directory))
        {
            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }

        PassiveTreeData tree = AssetDatabase.LoadAssetAtPath<PassiveTreeData>(OutputPath);

        if (tree != null)
            return tree;

        tree = ScriptableObject.CreateInstance<PassiveTreeData>();
        AssetDatabase.CreateAsset(tree, OutputPath);

        return tree;
    }

    private static void ImportNodes(
        PassiveTreeData tree,
        PassiveNodeJson[] source)
    {
        if (source == null || source.Length == 0)
            return;

        Vector2 min = new(float.MaxValue, float.MaxValue);
        Vector2 max = new(float.MinValue, float.MinValue);

        foreach (PassiveNodeJson node in source)
        {
            Vector2 position = new(node.Position.X, node.Position.Y);
            min = Vector2.Min(min, position);
            max = Vector2.Max(max, position);
        }

        Vector2 center = (min + max) * 0.5f;

        foreach (PassiveNodeJson node in source)
        {
            Vector2 position = new(node.Position.X, node.Position.Y);

            PassiveNode destination = new()
            {
                Name = node.Name,
                Type = node.Type,
                Position = position - center,
                Role = node.Role,
                Cluster = node.Cluster
            };

            if (node.Cost != null)
            {
                foreach (PassiveCostJson cost in node.Cost)
                {
                    destination.Cost.Add(new PassiveNodeCost
                    {
                        Amount = cost.Amount,
                        Attribute = new GameTag(CleanTag(cost.Attribute?.TagName))
                    });
                }
            }

            if (node.Requirement != null)
            {
                foreach (PassiveCostJson requirement in node.Requirement)
                {
                    destination.Requirement.Add(new PassiveNodeCost
                    {
                        Amount = requirement.Amount,
                        Attribute = new GameTag(CleanTag(requirement.Attribute?.TagName))
                    });
                }
            }

            tree.Nodes.Add(destination);
        }
    }

    private static void ImportNodeTypes(
        PassiveTreeData tree,
        PassiveNodeTypeJson[] source)
    {
        if (source == null) return;

        foreach (PassiveNodeTypeJson type in source)
        {
            PassiveNodeType destination = new()
            {
                Name = type.Name,
                Texture = FindTexture(type.Texture)
            };

            if (type.Modifier != null)
            {
                foreach (PassiveModifierJson modifier in type.Modifier)
                {
                    if (modifier?.Modifier == null || string.IsNullOrWhiteSpace(modifier.Modifier.TagName))
                    {
                        throw new InvalidDataException(
                            $"[Import Failed] Node type '{type.Name}' has a modifier with a null or empty TagName.");
                    }

                    string cleanTagStr = CleanTag(modifier.Modifier.TagName);

                    if (!Enum.TryParse(modifier.MathOp, true, out MathOp mathOp))
                    {
                        throw new ArgumentException(
                            $"[Import Failed] Invalid MathOp '{modifier.MathOp}' on node type '{type.Name}'.");
                    }

                    destination.Modifiers.Add(new StatModifier(
                        new GameTag(cleanTagStr),
                        mathOp,
                        modifier.FinalValue
                    ));
                }
            }

            tree.NodeTypes.Add(destination);
        }
    }

    private static void ImportConnections(
        PassiveTreeData tree,
        PassiveNodeNeighborsJson[] source)
    {
        if (source == null) return;

        HashSet<string> connections = new();

        foreach (PassiveNodeNeighborsJson node in source)
        {
            if (node.Neighbors == null) continue;

            foreach (string neighbor in node.Neighbors)
            {
                string key = string.CompareOrdinal(node.Name, neighbor) < 0
                    ? $"{node.Name}|{neighbor}"
                    : $"{neighbor}|{node.Name}";

                if (!connections.Add(key))
                    continue;

                tree.Connections.Add(new PassiveNodeConnection
                {
                    A = node.Name,
                    B = neighbor
                });
            }
        }
    }

    private static string CleanTag(string rawTag)
    {
        if (string.IsNullOrWhiteSpace(rawTag))
            return string.Empty;

        int lastDot = rawTag.LastIndexOf('.');
        return (lastDot >= 0 && rawTag.EndsWith("'"))
            ? rawTag.Substring(lastDot + 1).TrimEnd('\'')
            : rawTag;
    }

    private static Sprite FindTexture(string uePath)
    {
        if (string.IsNullOrWhiteSpace(uePath))
            return null;

        string fileName = Path.GetFileNameWithoutExtension(uePath);

        string[] guids = AssetDatabase.FindAssets(
            $"{fileName} t:Sprite",
            new[] { "Assets/Textures/Icons" });

        if (guids.Length == 0)
        {
            Debug.LogError($"Could not find sprite for UE texture '{uePath}'.");
            return null;
        }

        if (guids.Length > 1)
        {
            Debug.LogWarning($"Multiple sprites found for '{fileName}', using first match.");
        }

        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    [System.Serializable]
    private class PassiveNodeJsonArray { public PassiveNodeJson[] Items; }

    [System.Serializable]
    private class PassiveNodeTypeJsonArray { public PassiveNodeTypeJson[] Items; }

    [System.Serializable]
    private class PassiveNodeNeighborsJsonArray { public PassiveNodeNeighborsJson[] Items; }

    [System.Serializable]
    private class PassiveNodeJson
    {
        public string Name;
        public string Type;
        public PositionJson Position;
        public PassiveNodeRole Role;
        public PassiveCostJson[] Cost;
        public string Cluster;
        public PassiveCostJson[] Requirement;
        public int Neighbors;
    }

    [System.Serializable]
    private class PositionJson
    {
        public float X;
        public float Y;
    }

    [System.Serializable]
    private class PassiveCostJson
    {
        public int Amount;
        public AttributeJson Attribute;
    }

    [System.Serializable]
    private class AttributeJson
    {
        public string TagName;
    }

    [System.Serializable]
    private class PassiveNodeTypeJson
    {
        public string Name;
        public PassiveModifierJson[] Modifier;
        public string Texture;
    }

    [System.Serializable]
    private class PassiveModifierJson
    {
        public float FinalValue;
        public string MathOp;
        public string Restriction;
        public AttributeJson Modifier;
    }

    [System.Serializable]
    private class PassiveNodeNeighborsJson
    {
        public string Name;
        public string[] Neighbors;
    }
}
#endif