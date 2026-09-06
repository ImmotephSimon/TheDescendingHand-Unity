using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipmentType", menuName = "Items/Equipment Type")]
public class EquipmentType : ScriptableObject
{
    private static readonly Dictionary<Guid, EquipmentType> Registry = new();

    [SerializeField] private Guid id;
    [SerializeField] private ModifierPool modifierPool;

    public Guid ID => id;
    public ModifierPool ModifierPool => modifierPool;

    private void OnEnable()
    {
        Registry[id] = this;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        string path = UnityEditor.AssetDatabase.GetAssetPath(this);
        if (!string.IsNullOrEmpty(path))
        {
            string hex = UnityEditor.AssetDatabase.AssetPathToGUID(path);
            id = Guid.Parse(hex);
        }
    }
#endif


    public static bool TryGet(Guid id, out EquipmentType type)
    {
        return Registry.TryGetValue(id, out type);
    }
}