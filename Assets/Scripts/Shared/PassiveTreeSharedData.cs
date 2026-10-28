using System;
using System.Collections.Generic;
using UnityEngine;

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

public enum PassiveNodeRole
{
    Unset,
    Starter,
    Keystone
}

[Serializable]
public class PassiveNodeCost
{
    public int Amount;
    public GameTag Attribute;
}


[Serializable]
public class PassiveNodeConnection
{
    public string A;
    public string B;
}

[Serializable]
public class PassiveTreeDto
{
    public List<PassiveNode> Nodes = new();
    public List<PassiveNodeConnection> Connections = new();
    public List<PassiveNodeTypeDto> NodeTypes = new();
}

[Serializable]
public class PassiveNodeTypeDto
{
    public string Name;
    public SpriteString Sprite;
    public string DisplayTitle;
    public string Description;
}