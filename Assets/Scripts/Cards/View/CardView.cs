using System;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] private TextMeshPro title;
    [SerializeField] private TextMeshPro description;
    [SerializeField] private TextMeshPro keybind;
    [SerializeField] private MeshRenderer art;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private CardDefinition definition;
    private Transform root;


    public void SetHandTransform(Vector3 position, Quaternion rotation)
    {
        targetPosition = position;
        targetRotation = rotation;
    }

    public void MoveTowardsHandTransform(float speed)
    {
        root.transform.localPosition = Vector3.Lerp(
            root.transform.localPosition,
            targetPosition,
            speed * Time.deltaTime);

        root.transform.localRotation = Quaternion.Lerp(
            root.transform.localRotation,
            targetRotation,
            speed * Time.deltaTime);
    }

    public void InitializeTooltipCard(CardDefinition definition)
    {
        this.definition = definition;
        ApplyVisuals(definition.Visuals);
    }

    public void InitializePhysicalCard(CardDefinition definition, Transform root, string keybindText)
    {
        this.definition = definition;
        this.root = root;
        this.keybind.SetText($"{keybindText}");
        ApplyVisuals(definition.Visuals);
    }

    private void ApplyVisuals(CardVisuals visuals)
    {
        title.text = visuals.Name;
        description.text = visuals.Description;
        art.material.mainTexture = visuals.Art;
    }

    public void UpdateParent(Transform parent)
    {
        root.transform.SetParent(parent, true);
        root.localScale = Vector3.one * 0.2f;
    }
    public void DestroyCard()
    {
        Destroy(root.gameObject);
    }


    // hover, selection, highlight, etc. stay here
}