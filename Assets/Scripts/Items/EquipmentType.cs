using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipmentType", menuName = "Items/Equipment Type")]
public class EquipmentType : ScriptableObject
{
    private static readonly Dictionary<string, EquipmentType> Registry = new();

    [SerializeField] private string id;
    [SerializeField] private ModifierPool modifierPool;

    public string ID => id;
    public ModifierPool ModifierPool => modifierPool;

    private void OnEnable()
    {
        Registry[id] = this;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
            id = ItemDefinition.IdFormat(name);
    }
#endif


    public static bool TryGet(string id, out EquipmentType type)
    {
        return Registry.TryGetValue(id, out type);
    }
}