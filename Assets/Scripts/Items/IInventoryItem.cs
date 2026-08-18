using System;
using UnityEngine;

public interface IInventoryItem
{
    Guid Id { get; }
    Vector2Int Size { get; }
    Sprite Icon { get; }
}