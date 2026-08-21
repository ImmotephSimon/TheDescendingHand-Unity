using UnityEngine;
using UnityEngine.InputSystem;

public interface IPlayerMovement
{
    Vector3 Position { get; }

    void DodgeRoll();
    void SetLocalInput(Vector2 input, Vector3 mouseWorldPosition);
}