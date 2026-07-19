using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class PlayerInputController : MonoBehaviour
{
    private Vector2 _rawInput;
    private LayerMask _floorLayer;
    private GameInput _gameInput;
    private ClientBridge _clientBridge;

    private void Awake()
    {
        _floorLayer = LayerMask.GetMask("Floor");
        _gameInput = new GameInput();
        _gameInput.Enable();
        _gameInput.Player.Card.performed += OnPlayCard;
        _clientBridge = GetComponentInParent<ClientBridge>();
    }


    private void Update()
    {
        UpdateMovementInput();
    }



    private void OnPlayCard(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;
        Debug.Log($"Playing index 0");
        _clientBridge.AbilitySystem.RequestUseAbility(0);
        
    }
    private void UpdateMovementInput()
    {
        if (_clientBridge.Movement == null) return;

        _rawInput = _gameInput.Player.Move.ReadValue<Vector2>();

        Vector3 mouseWorldPosition = Vector3.zero;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _floorLayer))
        {
            mouseWorldPosition = hit.point;
        }
        else
        {
            Plane plane = new Plane(Vector3.up, _clientBridge.Movement.Position.y);
            if (plane.Raycast(ray, out float distance)) mouseWorldPosition = ray.GetPoint(distance);
        }

        _clientBridge.Movement.SetLocalInput(_rawInput, mouseWorldPosition);
    }


    private void OnDestroy()
    {
        _gameInput.Disable();
        _gameInput.Dispose();
    }
}