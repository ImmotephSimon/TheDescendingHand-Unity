using System;
using System.Collections.Generic;
using UnityEngine;

public class CardItemInstance : IInventoryItem
{
    public Guid InstanceId { get; } = Guid.NewGuid();



    public CardDefinition Definition { get; }

    public Vector2Int Size => new Vector2Int(1, 1);

    public Sprite Icon => Definition.Visuals.Icon;


    // Optional: Components list if cards share right-click/use logic with items
    public List<ItemUseComponent> Components { get; } = new();

    public Guid InventoryId => Definition.Id;

    public CardItemInstance(CardDefinition definition)
    {
        Definition = definition;
    }

    public void Use(IEntity owner)
    {
        // Execute card learning, deck addition, or casting logic here
    }

    public void Display()
    {
        throw new NotImplementedException();
    }
}