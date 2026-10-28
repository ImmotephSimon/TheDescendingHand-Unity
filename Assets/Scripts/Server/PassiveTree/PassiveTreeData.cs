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


[Serializable]
public class PassiveNodeType
{
    public string Name;
    public List<StatModifier> Modifiers = new();
    public Sprite Texture;
    public string DescriptionOverride;
}
