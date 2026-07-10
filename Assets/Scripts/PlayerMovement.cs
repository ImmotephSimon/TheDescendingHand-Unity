using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float debugSpeed = 0;
    [SerializeField] private float debugAnimSpeed = 0;
    private CharacterController controller;
    private Vector3 velocity;
    private Vector2 rawInput;
    private Animator animator;
    private Vector3 mouseWorldPosition;
    private LayerMask floorLayer;

    private Vector3 currentMoveVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        floorLayer = LayerMask.GetMask("Floor");

    }
    void Start()
    {
    }

    public void OnMove(InputValue value)
    {
        rawInput = value.Get<Vector2>();
    }

    void Update()
    {
        UpdateMousePosition();
        RotateTowardsMouse();

        float inputMagnitude = rawInput.magnitude;

        Vector3 movementInput = new Vector3(rawInput.x, 0, rawInput.y).normalized;

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        MoveCharacter(movementInput);
        UpdateAnimatorParameters();

        Vector3 localMovement = transform.InverseTransformDirection(movementInput);
        animator.SetFloat("MoveX", localMovement.x);
        animator.SetFloat("MoveY", localMovement.z);

    }

    private void UpdateAnimatorParameters()
    {
        animator.SetFloat("Speed", currentMoveVelocity.magnitude);

        float currentVelocity = currentMoveVelocity.magnitude;

        float animSpeed = currentVelocity > 0.1f
            ? currentVelocity / speed
            : 1f;

        animator.SetFloat("AnimSpeed", animSpeed);
        debugAnimSpeed = animSpeed;
    }

    void MoveCharacter(Vector3 movementInput)
    {
        Vector3 targetVelocity = movementInput * speed;

        float acceleration = 20f;
        float deceleration = 200f;

        float rate = movementInput == Vector3.zero
            ? deceleration
            : acceleration;

        currentMoveVelocity = Vector3.MoveTowards(
            currentMoveVelocity,
            targetVelocity,
            rate * Time.deltaTime
        );

        Vector3 displacement = currentMoveVelocity + velocity;
        controller.Move(displacement * Time.deltaTime);
        debugSpeed = currentMoveVelocity.magnitude;
    }

    void UpdateMousePosition()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());


        if (Physics.Raycast(ray, out RaycastHit hit, 100f, floorLayer))
        {
            mouseWorldPosition = hit.point;
        }
        else
        {
            Plane plane = new Plane(Vector3.up, transform.position.y);

            if (plane.Raycast(ray, out float distance))
            {
                mouseWorldPosition = ray.GetPoint(distance);
            }
        }
    }

    void RotateTowardsMouse()
    {
        Vector3 direction = mouseWorldPosition - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}