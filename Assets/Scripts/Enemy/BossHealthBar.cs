using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour, IHealthBar
{
    private Image _fill;
    private IHealth _health;

    private float _displayedHealth;
    private float _targetHealth;
    private float _degenStartHealth;
    private float _degenTimer;
    private const float DegenDuration = 0.25f;

    private void Awake()
    {
        _fill = GetComponentsInChildren<Image>(true).FirstOrDefault(x => x.name == "Fill");
    }

    public void Bind(IHealth health)
    {
        _health = health;
        _health.OnHealthChanged += UpdateHealth;

        // Initialize state directly on bind
        _displayedHealth = _health.CurrentHealth;
        _targetHealth = _health.CurrentHealth;

        if (_fill != null && _health.MaxHealth > 0)
        {
            _fill.fillAmount = _displayedHealth / _health.MaxHealth;
        }
    }

    public void Unbind()
    {
        if (_health == null)
            return;

        _health.OnHealthChanged -= UpdateHealth;
        _health = null;
    }

    private void UpdateHealth(float current, float max, bool isInstant)
    {
        if (isInstant)
        {
            _displayedHealth = current;
            _targetHealth = current;
            if (_fill != null && max > 0) _fill.fillAmount = current / max;
        }
        else
        {
            _degenStartHealth = _displayedHealth;
            _targetHealth = current;
            _degenTimer = 0f;
        }
    }

    private void LateUpdate()
    {
        if (_health == null || _displayedHealth == _targetHealth) return;

        _degenTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_degenTimer / DegenDuration);
        _displayedHealth = Mathf.Lerp(_degenStartHealth, _targetHealth, t);

        if (_fill != null && _health.MaxHealth > 0)
        {
            _fill.fillAmount = _displayedHealth / _health.MaxHealth;
        }
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnHealthChanged -= UpdateHealth;
    }
}