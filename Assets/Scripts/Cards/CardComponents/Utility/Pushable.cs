using UnityEngine;
using System.Collections.Generic;

public class Pushable : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float pushForce = 700f;
    private List<Transform> interactingEntities = new List<Transform>();

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (rb == null)
        {
            Debug.LogError($"No Rigidbody found on parent ({transform.parent?.name}) or current object ({gameObject.name}).", gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IEntity>(out var entity))
        {
            if (!interactingEntities.Contains(other.transform))
            {
                interactingEntities.Add(other.transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IEntity>(out var entity))
        {
            interactingEntities.Remove(other.transform);
        }
    }

    private void FixedUpdate()
    {
        if (!FishNet.InstanceFinder.IsServerStarted) return;
        if (rb == null || interactingEntities.Count == 0) return;

        // Clean up any nulls just in case an entity gets destroyed while inside
        //interactingEntities.RemoveAll(item => item == null);

        foreach (var entityTransform in interactingEntities)
        {
            Vector3 pushDir = (transform.position - entityTransform.position).normalized;
            pushDir.y = 0;

            rb.AddForce(pushDir * pushForce, ForceMode.Force);
        }
    }
}