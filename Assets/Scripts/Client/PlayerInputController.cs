using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class PlayerInputController : MonoBehaviour
{
    private Vector2 _rawInput;
    private LayerMask _floorLayer;
    private GameInput _gameInput;

    private void Awake()
    {
        _floorLayer = LayerMask.GetMask("Floor");
        _gameInput = new GameInput();
        _gameInput.Enable();

        _gameInput.Player.Card1.performed += ctx => OnPlayCard(0);
        _gameInput.Player.Card2.performed += ctx => OnPlayCard(1);
        _gameInput.Player.Card3.performed += ctx => OnPlayCard(2);
        _gameInput.Player.Card4.performed += ctx => OnPlayCard(3);
        _gameInput.Player.Card5.performed += ctx => OnPlayCard(4);


        _gameInput.Player.Card1.canceled += ctx => OnReleaseCard(0);
        _gameInput.Player.Card2.canceled += ctx => OnReleaseCard(1);
        _gameInput.Player.Card3.canceled += ctx => OnReleaseCard(2);
        _gameInput.Player.Card4.canceled += ctx => OnReleaseCard(3);
        _gameInput.Player.Card5.canceled += ctx => OnReleaseCard(4);

        
        _gameInput.Player.LeftClick.performed += OnLeftClick;
        _gameInput.Player.Interact.performed += OnLoot;
        _gameInput.Player.Inventory.performed += OnOpenInventory;

        _gameInput.Player.Dodgeroll.performed += DodgeRoll;

    }

    private void DodgeRoll(InputAction.CallbackContext context)
    {
        ClientBridge.Instance.Movement.DodgeRoll();
    }

    private void Update()
    {
        UpdateMovementInput();
    }

    private void OnPlayCard(int index)
    {
        Debug.Log($"Playing index {index}");
        ClientBridge.Instance.AbilitySystem.RequestUseAbility(index);
    }

    private void OnReleaseCard(int index)
    {
        ClientBridge.Instance.AbilitySystem.RequestCancelAbility(index);
    }

    private void UpdateMovementInput()
    {
        IPlayerMovement movement = ClientBridge.Instance.Movement;

        if (movement == null) return;

        _rawInput = _gameInput.Player.Move.ReadValue<Vector2>();

        Vector3 mouseWorldPosition = Vector3.zero;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _floorLayer))
        {
            mouseWorldPosition = hit.point;
        }
        else
        {
            Plane plane = new Plane(Vector3.up, movement.Position.y);
            if (plane.Raycast(ray, out float distance)) mouseWorldPosition = ray.GetPoint(distance);
        }

        movement.SetLocalInput(_rawInput, mouseWorldPosition);
    }

    private void OnLeftClick(InputAction.CallbackContext ctx)
    {
        CursorItemController.Instance?.TryDropHeldItem();
    }

    private void OnLoot(InputAction.CallbackContext ctx)
    {
        ClientBridge.Instance?.ClientPlayer?.TryInteract();
    }

    private void OnOpenInventory(InputAction.CallbackContext context)
    {
        ClientBridge.Instance.PlayerHUD.ToggleInventory();
    }

    private void OnDestroy()
    {
        _gameInput.Disable();
        _gameInput.Dispose();
    }
}