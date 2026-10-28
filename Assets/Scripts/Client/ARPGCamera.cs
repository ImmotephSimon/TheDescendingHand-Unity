using FishNet;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;

[ExecuteAlways]
public class ARPGCamera : MonoBehaviour
{
    [SerializeField] private float cameraDistance = 8f;
    private readonly float cameraAngle = 55f;

    private Vector3 Offset =>
        Quaternion.Euler(cameraAngle, 0f, 0f) * new Vector3(0f, 0f, -cameraDistance);
    private readonly InputAction _click = new("Click", InputActionType.Button, "<Mouse>/leftButton");
    private Transform playerTransform;
    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }


    private void OnEnable()
    {
        _click.performed += OnClick;
        _click.Enable();
    }
    private void OnDisable()
    {
        _click.performed -= OnClick;
        _click.Disable();
    }

    private void Update()
    {
        if (playerTransform == null)
            FindPlayerTarget();

        if (playerTransform != null)
            SnapToPlayer();
    }

    private void FindPlayerTarget()
    {
        if (Application.isPlaying)
        {
            var firstObject = InstanceFinder.ClientManager?.Connection?.FirstObject;

            if (firstObject != null)
                playerTransform = firstObject.transform;
        }
        else
        {
            var movement = FindAnyObjectByType<PlayerMovementController>();

            if (movement != null)
                playerTransform = movement.transform;
        }

    }

    private void SnapToPlayer()
    {
        base.transform.position = playerTransform.position + Offset;

        Shader.SetGlobalVector("_PlayerPosition", playerTransform.position + Vector3.up);

        Vector2 screenPos = Camera.main.WorldToViewportPoint(playerTransform.position);
        Shader.SetGlobalVector("_PlayerScreenPosition", screenPos);

        Shader.SetGlobalVector("_CameraPosition", base.transform.position);
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
            Debug.Log($"Click {hit.collider.gameObject.name}");
    }
}