using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipmentType", menuName = "Items/Equipment Type")]
public class EquipmentType : ScriptableObject
{
    [SerializeField] private ModifierPool modifierPool;

    public ModifierPool ModifierPool => modifierPool;
}