using UnityEngine;
[System.Serializable]
public abstract class Restriction : ScriptableObject
{
    public abstract bool AppliesTo(ItemDefinition item); // useful??
}