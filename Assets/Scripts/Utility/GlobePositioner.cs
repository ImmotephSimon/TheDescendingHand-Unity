using UnityEngine;

public class GlobePositioner : MonoBehaviour
{
    

    [SerializeField] private Transform healthGlobePrefab;
    [SerializeField] private Transform manaGlobePrefab;

    private readonly Vector2 healthScreenPosition = new(400, 150);
    private readonly Vector2 manaScreenPosition = new(1520, 150);

    private const float ReferenceWidth = 1920f;
    private const float ReferenceHeight = 1080f;

    private float distance = 1f;
    private Transform _healthGlobe;
    private Transform _manaGlobe;
    private Camera _targetCamera;
    private LiquidWobble[] liquids;
    private void Awake()
    {
        _targetCamera = Camera.main;
        _healthGlobe = Instantiate(healthGlobePrefab, transform);
        _manaGlobe = Instantiate(manaGlobePrefab, transform);
        liquids = GetComponentsInChildren<LiquidWobble>();

        if (liquids.Length == 0)
            Debug.LogError("ARPGCamera has no LiquidWobble children");
    }
    public void Initialize(PlayerStatsSync state)
    {
        
        foreach (var liquid in liquids)
        {
            liquid.Initialize(state);
        }
    }

    private void LateUpdate()
    {
        Place(_healthGlobe, healthScreenPosition);
        Place(_manaGlobe, manaScreenPosition);
    }

    private void Place(Transform globe, Vector2 screenPosition)
    {
        Vector2 viewport = new(
            screenPosition.x / ReferenceWidth,
            screenPosition.y / ReferenceHeight
        );

        globe.position = _targetCamera.ViewportToWorldPoint(
            new Vector3(viewport.x, viewport.y, distance)
        );
    }
}