using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewModifierPool", menuName = "Items/Modifier Pool")]
public class ModifierPool : ScriptableObject
{
    public List<ModifierPoolEntry> Entries = new();
}