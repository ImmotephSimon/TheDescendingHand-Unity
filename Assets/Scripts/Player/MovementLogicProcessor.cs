using UnityEngine;
using static PlayerMovement;

public class MovementLogicProcessor
{
    private readonly float _speed = 6f;
    private readonly float _dodgeSpeed = 12f;
    private readonly float _dodgeDuration = 0.4f;

    public MovementAction CurrentAction;
    public float ActionTimer;

    public Vector3 CalculateVelocity(MoveData md, Vector3 currentVelocity, float delta)
    {
        // 1. Handle Cancellations
        if (md.WantToCancel && CurrentAction == MovementAction.DodgeRoll)
        {
            CurrentAction = MovementAction.None;
            ActionTimer = 0f;
        }

        // 2. Handle Action State Transitions
        if (CurrentAction == MovementAction.None && md.ActionRequested == MovementAction.DodgeRoll)
        {
            CurrentAction = MovementAction.DodgeRoll;
            ActionTimer = _dodgeDuration;
        }

        // 3. Process Active State Mechanics
        if (CurrentAction == MovementAction.DodgeRoll)
        {
            ActionTimer -= delta;
            if (ActionTimer <= 0) CurrentAction = MovementAction.None;

            // Dodge roll moves forward regardless of directional input changes
            return currentVelocity.normalized * _dodgeSpeed;
        }

        // Default Normal Movement
        Vector3 moveInput = new Vector3(md.Input.x, 0, md.Input.y).normalized;
        return moveInput * _speed;
    }
}