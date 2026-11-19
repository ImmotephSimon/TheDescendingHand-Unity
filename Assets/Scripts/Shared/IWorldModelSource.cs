using System;
using UnityEngine;

public interface IWorldModelSource
{
    Guid Id { get; }       // null/empty = skip
    GameObject WorldModel { get; }  // null = skip
}