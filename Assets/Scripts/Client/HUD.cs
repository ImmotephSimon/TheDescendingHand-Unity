using System.Collections;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] private ProgressBarComponent experienceBar;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text manaText;
    [SerializeField] private GameObject speechBubble;
    [SerializeField] private TMP_Text zoneText;


    private const float DisplayDuration = 0.2f;

    private float _displayedHealth;
    private float _targetHealth;
    private float _maxHealth;
    private float _healthTimer;

    private float _displayedMana;
    private float _targetMana;
    private float _maxMana;
    private float _manaTimer;
    private Coroutine _speechCoroutine;

    private void Awake()
    {
        ClientRoot.Instance.OnClientPlayerReady += Initialize;
    }

    private void Initialize(ClientPlayer player)
    {
        PlayerStatsNetwork stats = player.GetComponent<PlayerStatsNetwork>();

        stats.ZoneLevel.OnChange += UpdateZoneText;
        UpdateZoneText(0, stats.ZoneLevel.Value, false);

        stats.Level.OnChange += (_, _, asServer) => { if (!asServer) UpdateExperience(stats); };
        stats.LevelProgress.OnChange += (_, _, asServer) => { if (!asServer) UpdateExperience(stats); };
        UpdateExperience(stats);

        stats.Health.OnChange += (prev, next, asServer) =>
        { if (!asServer) SetHealth(prev, next, stats.MaxHealth.Value, stats.HealthPerSecond.Value); };
        stats.MaxHealth.OnChange += (_, next, asServer) =>
        { if (!asServer) SetHealth(stats.Health.Value, stats.Health.Value, next, 0f); };
        SetHealth(stats.Health.Value, stats.Health.Value, stats.MaxHealth.Value, 0f);

        stats.Mana.OnChange += (prev, next, asServer) =>
        { if (!asServer) SetMana(prev, next, stats.MaxMana.Value, stats.ManaPerSecond.Value); };
        stats.MaxMana.OnChange += (_, next, asServer) =>
        { if (!asServer) SetMana(stats.Mana.Value, stats.Mana.Value, next, 0f); };
        SetMana(stats.Mana.Value, stats.Mana.Value, stats.MaxMana.Value, 0f);
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

    private void UpdateExperience(PlayerStatsNetwork s) =>
    experienceBar.SetValue(s.Level.Value, s.LevelProgress.Value);

    private void SetHealth(float prev, float next, float max, float rate)
    {
        _maxHealth = max;
        _targetHealth = next;

        if (WithRate(prev, next, rate)) _healthTimer = 0f;
        else { _displayedHealth = next; _healthTimer = DisplayDuration; }

        UpdateHealthText();
    }

    private void SetMana(float prev, float next, float max, float rate)
    {
        _maxMana = max;
        _targetMana = next;

        if (WithRate(prev, next, rate)) _manaTimer = 0f;
        else { _displayedMana = next; _manaTimer = DisplayDuration; }

        UpdateManaText();
    }

    private static bool WithRate(float prev, float next, float rate) =>
        (next > prev && rate > 0) || (next < prev && rate < 0);

    private void UpdateZoneText(int old, int current, bool asServer)
    {
        zoneText.SetText(
            $"Monster level: {current}");
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

    public void ShowSpeechBubble(string text, float duration)
    {
        var speechText = speechBubble.GetComponentInChildren<TMP_Text>();
        speechText.SetText(text);
        speechBubble.SetActive(true);

        if (_speechCoroutine != null)
            StopCoroutine(_speechCoroutine);

        if (duration == -1f) duration = CalculateSpeechDuration(text);
        _speechCoroutine = StartCoroutine(HideSpeechBubble(duration));
    }

    private IEnumerator HideSpeechBubble(float duration)
    {
        yield return new WaitForSeconds(duration);
        speechBubble.SetActive(false);

        _speechCoroutine = null;
    }

    private float CalculateSpeechDuration(string text) =>
    Mathf.Clamp(text.Length * 0.06f, 1.5f, 4f);
}