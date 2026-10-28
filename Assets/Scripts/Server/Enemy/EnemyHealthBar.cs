using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour, IHealthBar
{
    private const float _visibleDuration = 5f;
    private const float _degenDisplayDuration = 0.2f;

    private Camera cam;
    private Image fill;
    private IHealth _health;

    private float _displayedHealth;
    private float _targetHealth;
    private float _degenStartHealth;
    private float _degenTimer;
    private float hideTime;

    private void Awake()
    {
        cam = Camera.main;
        
    }
    private void Start()
    {
        fill = GetComponentsInChildren<Image>().First(x => x.name == "Fill");
        SetVisbility(false);
    }

    public void Bind(IHealth health)
    {
        _health = health;
        _health.OnHealthChanged += UpdateHealth;
    }

    private void UpdateHealth(float current, float max, bool isInstant)
    {
        if (current >= max)
        {
            SetVisbility(false);
            return;
        }

        if (isInstant)
        {
            _displayedHealth = current;
            _targetHealth = current;
            fill.fillAmount = current / max;
        }
        else
        {
            _degenStartHealth = _displayedHealth;
            _targetHealth = current;
            _degenTimer = 0f;
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

            fill.fillAmount = _displayedHealth / _health.MaxHealth;
        }

        if (Time.time >= hideTime)
            SetVisbility(false);
    }

    private void SetVisbility(bool IsVisible)
    {
        foreach (var g in GetComponentsInChildren<Graphic>())
            g.enabled = IsVisible;
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnHealthChanged -= UpdateHealth;
    }


}