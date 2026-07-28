using FishNet;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[ExecuteAlways]
public class ARPGCamera : MonoBehaviour
{
    [SerializeField] private float cameraDistance = 8f;
    private readonly float cameraAngle = 55f;

    private Vector3 Offset =>
        Quaternion.Euler(cameraAngle, 0f, 0f) * new Vector3(0f, 0f, -cameraDistance);

    private Transform playerTransform;
    private LiquidWobble[] liquids;
    private bool initialized;

    private void Awake()
    {
        liquids = GetComponentsInChildren<LiquidWobble>();

        if (liquids.Length == 0)
            Debug.LogError("ARPGCamera has no LiquidWobble children");
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

        if (!initialized && playerTransform != null)
        {
            var entity = playerTransform.transform.GetComponent<IEntity>();
            foreach (var liquid in liquids)
            { 
                liquid.Initialize(entity);
            }

            initialized = true;
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
}