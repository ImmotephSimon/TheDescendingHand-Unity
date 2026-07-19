using UnityEngine;

public abstract class Restriction : ScriptableObject
{
    public abstract bool AppliesTo(ItemDefinition item);
}