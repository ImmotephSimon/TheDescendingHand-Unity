using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Passive Tree/Passive Tree Data")]
public class PassiveTreeData : ScriptableObject
{
    public List<PassiveNode> Nodes = new();
    public List<PassiveNodeType> NodeTypes = new();
    public List<PassiveNodeConnection> Connections = new();
}

public enum PassiveNodeRole
{
    Unset,
    Starter,
    Keystone
}

[Serializable]
public class PassiveNode
{
    public string Name;
    public string Type;
    public Vector2 Position;
    public PassiveNodeRole Role;
    public List<PassiveNodeCost> Cost = new();
    public string Cluster;
    public List<PassiveNodeCost> Requirement = new();
}

[Serializable]
public class PassiveNodeCost
{
    public int Amount;
    public GameTag Attribute;
}

[Serializable]
public class PassiveNodeType
{
    public string Name;
    public List<StatModifier> Modifiers = new();
    public Sprite Texture;
}


[Serializable]
public class PassiveNodeConnection
{
    public string A;
    public string B;
}