using System.Collections.Generic;
using UnityEngine;

public class Pushable : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float pushForce = 700f;
    private List<Transform> interactingEntities = new List<Transform>();
    private int _playerLayer;
    private int _enemyLayer;

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
        _playerLayer = LayerMask.NameToLayer("Player");
        _enemyLayer = LayerMask.NameToLayer("Enemy");

    }



    private bool CanPush(Collider other) =>
        other.gameObject.layer == _playerLayer ||
        other.gameObject.layer == _enemyLayer;

    private void OnTriggerEnter(Collider other)
    {
        if (!CanPush(other)) return;

        if (!interactingEntities.Contains(other.transform))
            interactingEntities.Add(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!CanPush(other)) return;

        interactingEntities.Remove(other.transform);
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