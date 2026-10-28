using System;

public class EquipUseComponent : ItemUseComponent, IUsable
{
    public EquipComponentDefinition Definition { get; }

    public EquipUseComponent(EquipComponentDefinition definition)
    {
        Definition = definition;
    }

    public void Equip(ItemDropInstance instance, IEntity user)
    {
        instance.ApplyModifiers(user);
    }

    public void Unequip(ItemDropInstance instance, IEntity user)
    {
        instance.ClearModifiers(user);
    }

    public override void Use(ItemDropInstance instance, IEntity user)
    {
        throw new InvalidOperationException($"[Item] {instance.BaseType.name} is an equipment item and cannot be used directly.");
    }
}