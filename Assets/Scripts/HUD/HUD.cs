using System;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] private ExperienceBar experienceBar;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text manaText;

    public void Bind(LevelComponent comp)
    {

        comp.OnExperienceChanged += experienceBar.SetValue;
        experienceBar.SetValue(comp.Level, comp.Progress);
    }

    public void Bind(IHealth health)
    {
        health.OnHealthChanged += UpdateHealth;
        UpdateHealth(health.CurrentHealth, health.MaxHealth);
    }

    private void UpdateHealth(float current, float max)
    {
        healthText.SetText($"{current} / {max}");
        
    }

    public void Bind(IMana mana)
    {
        mana.OnManaChanged += UpdateMana;
        UpdateMana(mana.CurrentMana, mana.MaxMana);
    }

    private void UpdateMana(float current, float max)
    {
        manaText.SetText($"{current} / {max}");
    }

    
}