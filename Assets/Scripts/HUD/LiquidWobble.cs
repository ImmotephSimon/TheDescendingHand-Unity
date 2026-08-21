using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public enum LiquidGlobeType
{
    Life,
    Mana
}

public class LiquidWobble : MonoBehaviour
{
    [Header("Wobble Settings")]
    [SerializeField] private float maxWobble = 0.03f;
    [SerializeField] private float wobbleSpeed = 1.0f;
    [SerializeField] private float recovery = 1.0f;
    [SerializeField] private GameTag resourceTag;
    [SerializeField] private LiquidGlobeType type;

    [Header("Liquid State")]
    [Range(0f, 1f)] public float fillPercentage = 0.5f;
    public Color lightColor = Color.red;
    public Color darkColor = new Color(0.2f, 0f, 0f);

    private Renderer rend;
    private MaterialPropertyBlock propBlock;

    private float targetFillPercentage;
    private float fillTimer;
    private const float FillDuration = 0.2f;
    private Vector3 lastPos;
    private Vector3 velocity;
    private Vector3 wobbleAmount;
    private Vector3 wobbleAmountToAdd;
    private float pulse;
    private float time;

    private void OnValidate()
    {
        switch (type)
        {
            case LiquidGlobeType.Life:
                lightColor = Color.red;
                darkColor = new Color(0.2f, 0f, 0f);
                break;

            case LiquidGlobeType.Mana:
                lightColor = Color.blue;
                darkColor = new Color(0f, 0f, 0.2f);
                break;
        }
        UpdatePropertyBlock(new Vector3());
    }

    void Start()
    {
        lastPos = transform.position;
        targetFillPercentage = fillPercentage;
    }

    void Update()
    {
        time += Time.deltaTime;

        // 1. Calculate Velocity
        velocity = (transform.position - lastPos) / Time.deltaTime;

        // 2. Add Velocity to Wobble (Clamp to prevent extreme stretching)
        wobbleAmountToAdd.x += Mathf.Clamp((velocity.x + (velocity.z * 0.2f)) * maxWobble, -maxWobble, maxWobble);
        wobbleAmountToAdd.z += Mathf.Clamp((velocity.z + (velocity.x * 0.2f)) * maxWobble, -maxWobble, maxWobble);

        // 3. Sine wave oscillation to decay/stabilize the wobble back to zero
        wobbleAmount.x = Mathf.Lerp(wobbleAmount.x, 0, Time.deltaTime * recovery);
        wobbleAmount.z = Mathf.Lerp(wobbleAmount.z, 0, Time.deltaTime * recovery);

        pulse = 2 * Mathf.PI * wobbleSpeed;
        wobbleAmountToAdd.x = Mathf.Lerp(wobbleAmountToAdd.x, 0, Time.deltaTime * recovery);
        wobbleAmountToAdd.z = Mathf.Lerp(wobbleAmountToAdd.z, 0, Time.deltaTime * recovery);

        float fillMultiplier = Mathf.Lerp(1.5f, 0.5f, fillPercentage);

        Vector3 currentTilt = new Vector3(
            (wobbleAmount.x + Mathf.Sin(pulse * time) * wobbleAmountToAdd.x) * fillMultiplier,
            0,
            (wobbleAmount.z + Mathf.Cos(pulse * time) * wobbleAmountToAdd.z) * fillMultiplier
        );
        lastPos = transform.position;

        // Lerp degens
        if (fillTimer < FillDuration)
        {
            fillTimer += Time.deltaTime;
            fillPercentage = Mathf.Lerp(
                fillPercentage,
                targetFillPercentage,
                Time.deltaTime / (FillDuration - fillTimer + Time.deltaTime));
        }

        UpdatePropertyBlock(currentTilt);
    }

    private void UpdatePropertyBlock(Vector3 currentTilt)
    {
        if (rend == null) rend = GetComponent<Renderer>();
        if (propBlock == null) propBlock = new MaterialPropertyBlock();

        rend.GetPropertyBlock(propBlock);
        propBlock.SetVector("_CurrentTilt", currentTilt);
        propBlock.SetFloat("_Fill", fillPercentage);
        propBlock.SetColor("_LightColor", lightColor);
        propBlock.SetColor("_DarkColor", darkColor);
        rend.SetPropertyBlock(propBlock);
    }



    public void Initialize(PlayerStatsSync state)
    {
        switch (type)
        {
            case LiquidGlobeType.Life:
                state.HealthChanged += UpdateHealth;
                break;

            case LiquidGlobeType.Mana:
                state.ManaChanged += UpdateMana;
                break;
        }
    }

    private void UpdateMana(float current, float max, bool isInstant)
    {
        SetFill(current / max, isInstant);
    }

    private void UpdateHealth(float current, float max, bool isInstant)
    {
        SetFill(current / max, isInstant);
    }

    private void SetFill(float value, bool isInstant)
    {
        targetFillPercentage = value;

        if (isInstant)
        {
            fillPercentage = value;
            fillTimer = FillDuration;
        }
        else
        {
            fillTimer = 0f;
        }
    }
}