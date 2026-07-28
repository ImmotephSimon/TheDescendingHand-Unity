using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image fill;
    private float visibleDuration = 5f;

    private Camera cam;
    private IHealth health;
    private float hideTime;

    private void Awake()
    {
        cam = Camera.main;
        SetVisbility(false);
    }

    public void Bind(IHealth target)
    {
        health = target;
        health.OnHealthChanged += UpdateHealth;
    }

    private void UpdateHealth(float current, float max)
    {
        fill.fillAmount = current / max;

        SetVisbility(true);
        hideTime = Time.time + visibleDuration;
    }

    private void LateUpdate()
    {
        transform.rotation = cam.transform.rotation;

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
        if (health != null)
            health.OnHealthChanged -= UpdateHealth;
    }


}