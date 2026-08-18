using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] private ExperienceBar experienceBar;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text manaText;
    [SerializeField] private RectTransform hudRoot;
    public RectTransform HUDRoot => hudRoot;

    private const float DisplayDuration = 0.2f;

    private float _displayedHealth;
    private float _targetHealth;
    private float _maxHealth;
    private float _healthTimer;

    private float _displayedMana;
    private float _targetMana;
    private float _maxMana;
    private float _manaTimer;

    public void Bind(LevelComponent comp)
    {
        comp.OnExperienceChanged += experienceBar.SetValue;
        experienceBar.SetValue(comp.Level, comp.Progress);
    }

    public void Bind(IHealth health)
    {
        health.OnHealthChanged += UpdateHealth;
        UpdateHealth(health.CurrentHealth, health.MaxHealth, true);
    }

    public void Bind(IMana mana)
    {
        mana.OnManaChanged += UpdateMana;
        UpdateMana(mana.CurrentMana, mana.MaxMana, true);
    }

    private void UpdateHealth(float current, float max, bool isInstant)
    {
        _targetHealth = current;
        _maxHealth = max;

        if (isInstant)
        {
            _displayedHealth = current;
            _healthTimer = DisplayDuration;
        }
        else
        {
            _healthTimer = 0f;
        }

        UpdateHealthText();
    }

    private void UpdateMana(float current, float max, bool isInstant)
    {
        _targetMana = current;
        _maxMana = max;

        if (isInstant)
        {
            _displayedMana = current;
            _manaTimer = DisplayDuration;
        }
        else
        {
            _manaTimer = 0f;
        }

        UpdateManaText();
    }

    private void Update()
    {
        if (_healthTimer < DisplayDuration)
        {
            _healthTimer += Time.deltaTime;
            _displayedHealth = Mathf.Lerp(
                _displayedHealth,
                _targetHealth,
                Time.deltaTime / (DisplayDuration - _healthTimer + Time.deltaTime));

            UpdateHealthText();
        }

        if (_manaTimer < DisplayDuration)
        {
            _manaTimer += Time.deltaTime;
            _displayedMana = Mathf.Lerp(
                _displayedMana,
                _targetMana,
                Time.deltaTime / (DisplayDuration - _manaTimer + Time.deltaTime));

            UpdateManaText();
        }
    }

    private void UpdateHealthText()
    {
        healthText.SetText(
            $"{Mathf.RoundToInt(_displayedHealth)} / {Mathf.RoundToInt(_maxHealth)}");
    }

    private void UpdateManaText()
    {
        manaText.SetText(
            $"{Mathf.RoundToInt(_displayedMana)} / {Mathf.RoundToInt(_maxMana)}");
    }
}