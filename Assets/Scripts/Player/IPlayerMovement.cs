using UnityEngine;

public interface IPlayerMovement
{
    Vector3 Position { get; }
    void SetLocalInput(Vector2 input, Vector3 mouseWorldPosition);
}