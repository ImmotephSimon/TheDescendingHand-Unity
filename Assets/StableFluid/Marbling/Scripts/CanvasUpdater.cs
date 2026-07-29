using StableFluids.Marbling;
using System.Collections;
using UnityEngine;

public class CanvasUpdater : MonoBehaviour
{
    [SerializeField] MarblingController controller;
    [SerializeField] Transform cameraTransform;

    void Awake()
    {
        var renderer = GetComponent<Renderer>();

        if (renderer == null)
        {
            Debug.LogError($"{name}: No Renderer found.");
            return;
        }
        renderer.material.mainTexture = controller.Canvas;
    }

    void Start()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

}