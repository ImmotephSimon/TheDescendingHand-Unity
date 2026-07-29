using System;
using UnityEngine;

public class CardView : MonoBehaviour
{
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private CardDefinition definition;
    private Transform outer;

    public void SetHandTransform(Vector3 position, Quaternion rotation)
    {
        targetPosition = position;
        targetRotation = rotation;
    }

    public void MoveTowardsHandTransform(float speed)
    {
        outer.transform.localPosition = Vector3.Lerp(
            outer.transform.localPosition,
            targetPosition,
            speed * Time.deltaTime);

        outer.transform.localRotation = Quaternion.Lerp(
            outer.transform.localRotation,
            targetRotation,
            speed * Time.deltaTime);
    }


    public void Initialize(CardDefinition definition)
    {
        this.definition = definition;

        // set icon, name, art, etc. here
    }

    public void UpdateParent(Transform parent)
    {
        if (outer == null) outer = transform.parent;
        outer.transform.SetParent(parent, false);
        outer.localScale = Vector3.one * 0.2f;
    }

    // hover, selection, highlight, etc. stay here
}