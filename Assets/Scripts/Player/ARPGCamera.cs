using FishNet;
using UnityEngine;

[ExecuteAlways]
public class ARPGCamera : MonoBehaviour
{
    [SerializeField] private float cameraDistance = 8f;
    private readonly float cameraAngle = 55f;

    private Vector3 Offset
    {
        get
        {
            Quaternion rotation = Quaternion.Euler(cameraAngle, 0f, 0f);
            return rotation * new Vector3(0f, 0f, -cameraDistance);
        }
    }
    private Transform player;

    void OnEnable()
    {
        FindPlayerTarget();
    }

    void Update()
    {
        // In the editor or if the connection dropped, fallback to searching the scene
        if (player == null)
        {
            FindPlayerTarget();
        }

        SnapToPlayer();
    }

    private void FindPlayerTarget()
    {
        // At runtime, explicitly find our owned local player connection
        if (Application.isPlaying && InstanceFinder.ClientManager?.Connection?.FirstObject != null)
        {
            player = InstanceFinder.ClientManager.Connection.FirstObject.transform;
            return;
        }

        // Editor fallback or initial scene setup search
        var movement = FindAnyObjectByType<PlayerMovement>();
        if (movement != null) player = movement.transform;
    }

    private void SnapToPlayer()
    {
        if (player != null)
        {
            transform.position = player.position + Offset;

            Shader.SetGlobalVector("_PlayerPosition", player.position + new Vector3(0, 1, 0));
            Vector2 screenPos = Camera.main.WorldToViewportPoint(player.position);
            Shader.SetGlobalVector("_PlayerScreenPosition", screenPos);
            // In your player or camera script Update loop:
            Shader.SetGlobalVector("_CameraPosition", transform.position);
        }
    }
}