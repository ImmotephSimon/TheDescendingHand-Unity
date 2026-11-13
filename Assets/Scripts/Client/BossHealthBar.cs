using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    private Image _fill;
    private EnemyStatsNetwork _stats;

    private float _displayedHealth;
    private float _targetHealth;
    private float _degenStartHealth;
    private float _degenTimer;
    private const float DegenDuration = 0.25f;

    private void Awake()
    {
        _fill = GetComponentsInChildren<Image>(true).FirstOrDefault(x => x.name == "Fill");
    }

    public void Bind(EnemyStatsNetwork stats)
    {
        _stats = stats;
        _stats.Health.OnChange += OnHealthChanged;
        _stats.MaxHealth.OnChange += OnMaxHealthChanged;

        // Initialize state directly on bind
        _displayedHealth = _targetHealth = _stats.Health.Value;
        RefreshFill();
    }

    public void Unbind()
    {
        if (_stats == null)
            return;

        _stats.Health.OnChange -= OnHealthChanged;
        _stats.MaxHealth.OnChange -= OnMaxHealthChanged;
        _stats = null;
    }

    private void OnHealthChanged(float prev, float next, bool asServer)
    {
        if (asServer) return;

        float delta = next - prev;
        float rate = _stats.HealthRate.Value;
        bool withRate = (delta > 0 && rate > 0) || (delta < 0 && rate < 0);

        if (withRate)
        {
            _degenStartHealth = _displayedHealth;
            _targetHealth = next;
            _degenTimer = 0f;
        }
        else
        {
            _displayedHealth = _targetHealth = next;
            RefreshFill();
        }
    }

    private void OnMaxHealthChanged(float prev, float next, bool asServer)
    {
        if (asServer) return;
        RefreshFill();
    }

    private void RefreshFill()
    {
        if (_fill != null && _stats != null && _stats.MaxHealth.Value > 0)
            _fill.fillAmount = _displayedHealth / _stats.MaxHealth.Value;
    }

    private void LateUpdate()
    {
        if (_stats == null || _displayedHealth == _targetHealth) return;

        _degenTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_degenTimer / DegenDuration);
        _displayedHealth = Mathf.Lerp(_degenStartHealth, _targetHealth, t);

        RefreshFill();
    }

    private void OnDestroy() => Unbind();
}