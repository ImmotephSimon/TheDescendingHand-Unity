using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    private const float _visibleDuration = 5f;
    private const float _degenDisplayDuration = 0.2f;

    private Camera cam;
    private Image fill;
    private EnemyStatsNetwork _stats;

    private float _displayedHealth;
    private float _targetHealth;
    private float _degenStartHealth;
    private float _degenTimer;
    private float hideTime;

    private void Awake()
    {
        cam = Camera.main;
        _stats = GetComponentInParent<EnemyStatsNetwork>();
        fill = GetComponentsInChildren<Image>().First(x => x.name == "Fill");
    }

    private void OnEnable()
    {
        _stats.Health.OnChange += OnHealthChanged;
        _stats.MaxHealth.OnChange += OnMaxHealthChanged;
    }

    private void OnDisable()
    {
        _stats.Health.OnChange -= OnHealthChanged;
        _stats.MaxHealth.OnChange -= OnMaxHealthChanged;
    }

    private void Start()
    {
        _displayedHealth = _targetHealth = _stats.Health.Value;
        SetVisbility(false);
    }

    private void OnHealthChanged(float prev, float next, bool asServer)
    {
        if (asServer) return;
        UpdateHealth(prev, next, _stats.MaxHealth.Value, _stats.HealthRate.Value);
    }

    private void OnMaxHealthChanged(float prev, float next, bool asServer)
    {
        if (asServer) return;
        UpdateHealth(_stats.Health.Value, _stats.Health.Value, next, 0f);
    }

    private void UpdateHealth(float prev, float current, float max, float rate)
    {
        if (current >= max)
        {
            SetVisbility(false);
            return;
        }

        float delta = current - prev;
        bool withRate = (delta > 0 && rate > 0) || (delta < 0 && rate < 0);

        if (withRate)
        {
            _degenStartHealth = _displayedHealth;
            _targetHealth = current;
            _degenTimer = 0f;
        }
        else
        {
            _displayedHealth = _targetHealth = current;
            fill.fillAmount = current / max;
        }

        SetVisbility(true);
        hideTime = Time.time + _visibleDuration;
    }

    private void LateUpdate()
    {
        transform.rotation = cam.transform.rotation;

        if (_displayedHealth != _targetHealth)
        {
            _degenTimer += Time.deltaTime;

            float t = Mathf.Clamp01(_degenTimer / _degenDisplayDuration);
            _displayedHealth = Mathf.Lerp(_degenStartHealth, _targetHealth, t);

            fill.fillAmount = _displayedHealth / _stats.MaxHealth.Value;
        }

        if (Time.time >= hideTime)
            SetVisbility(false);
    }

    private void SetVisbility(bool IsVisible)
    {
        foreach (var g in GetComponentsInChildren<Graphic>())
            g.enabled = IsVisible;
    }
}