using UnityEngine;

[System.Serializable]
public class EquipComponentDefinition : ItemComponentDefinition
{
    [SerializeField] private EquipmentType equipmentType;
    public EquipmentType EquipmentType => equipmentType;

    public override ItemUseComponent CreateRuntimeComponent()
    {
        return new EquipUseComponent(this);
    }
}