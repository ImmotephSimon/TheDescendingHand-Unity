using System;
using UnityEngine;

public class DebrisCollisionNotifier : MonoBehaviour
{
    private Action<GameObject, Vector3> _onHit;
    private float _minVelocity = 2f;

    public void Initialize(Action<GameObject, Vector3> onHit, float lifetime = 2.5f)
    {
        _onHit = onHit;
        Destroy(this, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude < _minVelocity)
        {
            Destroy(this);
            return;
        }

        if (!collision.gameObject.CompareTag("Debris"))
        {
            // Just invoke the hit action with target and point
            _onHit?.Invoke(collision.gameObject, collision.contacts[0].point);
            Destroy(this);
        }
    }
}