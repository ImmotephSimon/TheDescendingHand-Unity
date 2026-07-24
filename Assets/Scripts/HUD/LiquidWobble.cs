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

    private Vector3 lastPos;
    private Vector3 velocity;
    private Vector3 wobbleAmount;
    private Vector3 wobbleAmountToAdd;
    private float pulse;
    private float time;
    private IHealth healthHandler;
    private IMana manaHandler;

    void Start()
    {
        rend = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
        lastPos = transform.position;
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

        Vector3 currentTilt = new Vector3(
            wobbleAmount.x + Mathf.Sin(pulse * time) * wobbleAmountToAdd.x,
            0,
            wobbleAmount.z + Mathf.Cos(pulse * time) * wobbleAmountToAdd.z
        );

        lastPos = transform.position;

        // 4. Send Parameters to Shader efficiently using MaterialPropertyBlock
        rend.GetPropertyBlock(propBlock);
        propBlock.SetVector("_CurrentTilt", currentTilt);
        propBlock.SetFloat("_Fill", fillPercentage);
        propBlock.SetColor("_LightColor", lightColor);
        propBlock.SetColor("_DarkColor", darkColor);
        rend.SetPropertyBlock(propBlock);
    }



    public void Initialize(IEntity player)
    {
        switch (type)
        {
            case LiquidGlobeType.Life:
                healthHandler = player.Transform.GetComponent<IHealth>();
                healthHandler.OnHealthChanged += UpdateHealth;
                lightColor = Color.red;
                break;

            case LiquidGlobeType.Mana:
                manaHandler = player.Transform.GetComponent<IMana>();
                manaHandler.OnManaChanged += UpdateMana;
                lightColor = Color.blue;
                break;
        }
    }

    private void UpdateMana()
    {
        fillPercentage =
            manaHandler.CurrentMana /
            manaHandler.MaxMana;
    }

    private void UpdateHealth()
    {
        fillPercentage =
            healthHandler.CurrentHealth /
            healthHandler.MaxHealth;
    }
}