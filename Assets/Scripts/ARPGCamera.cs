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
        var controller = FindAnyObjectByType<PlayerController>();
        if (controller != null) player = controller.transform;
    }

    void Update()
    {
        SnapToPlayer();
    }

    private void SnapToPlayer()
    {
        if (player != null)
        {
            transform.position = player.position + Offset;

            // Pipe the raw world coordinates straight to the GPU
            Shader.SetGlobalVector("_PlayerPosition", player.position + new Vector3(0, 1, 0));
            //Debug.Log($"PlayerPos: {player.position}");
            Shader.SetGlobalVector("_CameraPosition", transform.position);
            //Debug.Log($"Shader Camera: {transform.position}, Unity Camera: {Camera.main.transform.position}");
            Vector2 screenPos = Camera.main.WorldToViewportPoint(player.position);
            Shader.SetGlobalVector("_PlayerScreenPosition", screenPos);
        }
    }
}