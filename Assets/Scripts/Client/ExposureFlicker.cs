using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class ExposureFlicker : MonoBehaviour
{
    [SerializeField] private float minExposure = -0.15f;
    [SerializeField] private float maxExposure = 0.05f;
    [SerializeField] private float speed = 15f;

    private ColorAdjustments colorAdjustments;

    private void Awake()
    {
        if (GetComponent<Volume>().profile.TryGet(out colorAdjustments))
        {
            colorAdjustments.postExposure.overrideState = true;
        }
    }

    private void Update()
    {
        if (colorAdjustments == null) return;

        float noise = Mathf.PerlinNoise(Time.time * speed, 0f);
        colorAdjustments.postExposure.value = Mathf.Lerp(minExposure, maxExposure, noise);
    }
}