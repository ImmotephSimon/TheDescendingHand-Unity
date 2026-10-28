using System;
using UnityEngine;

[Serializable]
public abstract class ItemComponentDefinition
{
    public abstract ItemUseComponent CreateRuntimeComponent();
}