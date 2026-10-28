using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipmentType", menuName = "Items/Equipment Type")]
public class EquipmentTypeDefinition : ScriptableObject
{
    [SerializeField] private EquipmentSlot slot;
    [SerializeField] private ModifierPool modifierPool;

    public EquipmentSlot Slot => slot;
    public ModifierPool ModifierPool => modifierPool;
}